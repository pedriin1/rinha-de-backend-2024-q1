namespace rinha_de_backend_2024_q1.DTOs;

public class Balance
{
    public int id { get; set; }
    public int total { get; set; }
    public DateTime data_extrato { get; set; }
    public int limite { get; set; }

    public Balance(int offset)
    {
        id = offset;
    }
}