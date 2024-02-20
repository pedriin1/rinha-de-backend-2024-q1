namespace rinha_de_backend_2024_q1.DTOs;

public class TransactionRequest
{
    public int Valor { get; set; }
    public char Tipo { get; set; }
    public string Descricao { get; set; }
}

public class TransactionRequestId
{
    public int id { get; set; }
    public int Valor { get; set; }
    public char Tipo { get; set; }
    public string Descricao { get; set; }

    public TransactionRequestId(int id, int valor, char tipo, string descricao)
    {
        Valor = valor;
        Tipo = tipo;
        Descricao = descricao;
    }
}

public class TransactionResponse
{
    public int Limite { get; set; }
    public int Saldo { get; set; }
}
