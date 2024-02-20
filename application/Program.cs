using rinha_de_backend_2024_q1.DTOs;
using Confluent.Kafka;


Balance[] usersBalances = new Balance[5];
for (int i = 0; i < usersBalances.Length; i++)
{
    usersBalances[i] = new Balance(i + 1);
}

var builder = WebApplication.CreateBuilder(args);
string kafkaTopicConsumer = builder.Configuration["Kafka:Consumer"];
string kafkaTopicProducer = builder.Configuration["Kafka:Producer"];
string kafkaTopicServer = builder.Configuration["Kafka:Server"];


var config = new ProducerConfig
{
    BootstrapServers = kafkaTopicServer // Replace with your Kafka server address
};


var consumerConfig = new ConsumerConfig
{
    GroupId = "rinha-de-backend-group",
    BootstrapServers = kafkaTopicServer,
    AutoOffsetReset = AutoOffsetReset.Earliest
};
var cts = new CancellationTokenSource();

Task.Run(() =>
{
    using (var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build())
    {
        consumer.Subscribe(kafkaTopicConsumer);

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                var consumeResult = consumer.Consume(cts.Token);

                var request =
                    System.Text.Json.JsonSerializer.Deserialize<TransactionRequestId>(consumeResult.Message.Value);
                Console.WriteLine($"Received message: {consumeResult.Message.Value}");

                usersBalances[request.id - 1].total = request.Tipo == 'c'
                    ? usersBalances[request.id - 1].total + request.Valor
                    : usersBalances[request.id - 1].total - request.Valor;
            }
        }
        catch (OperationCanceledException)
        {
            consumer.Close();
        }
    }
}, cts.Token);


builder.Services.AddEndpointsApiExplorer();
var app = builder.Build();
app.UseHttpsRedirection();


app.MapPost("/clientes/{id}/transacoes", async (int id, TransactionRequest request) =>
    {
        if (id <= 0 || request.Valor <= 0 || (request.Tipo != 'c' && request.Tipo != 'd') ||
            request.Descricao.Length > 10)
        {
            return Results.BadRequest("Invalid request parameters.");
        }

        usersBalances[id - 1].total = request.Tipo == 'c'
            ? usersBalances[id - 1].total + request.Valor
            : usersBalances[id - 1].total - request.Valor;


        using (var producer = new ProducerBuilder<Null, string>(config).Build())
        {
            try
            {
                var jsonBalances =
                    System.Text.Json.JsonSerializer.Serialize(new TransactionRequestId(id, request.Valor, request.Tipo,
                        request.Descricao));
                var message = new Message<Null, string> { Value = jsonBalances };
                var result = await producer.ProduceAsync(kafkaTopicProducer, message);

                Console.WriteLine($"Message sent to partition {result.Partition} with offset {result.Offset}");
            }
            catch (ProduceException<Null, string> e)
            {
                Console.WriteLine($"Failed to deliver message: {e.Message} [{e.Error.Code}]");
            }
        }

        var response = new TransactionResponse
        {
            Limite = usersBalances[id - 1].limite,
            Saldo = usersBalances[id - 1].total
        };

        return Results.Ok(response);
    })
    .WithName("PostTransaction")
    .WithOpenApi();


app.Run();

