namespace QuantIA.Models;

public class TransactionType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public TransactionDirection Direction { get; set; }

    public string Icon { get; set; } = null!;
}