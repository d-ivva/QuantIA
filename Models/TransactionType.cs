namespace QuantIA.Models;

public class TransactionType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Direction { get; set; } = null!; // "income" | "expense"

    public string Icon { get; set; } = null!;
}