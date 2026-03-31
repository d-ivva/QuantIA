namespace QuantIA.Models;

public class MonthlyBudget
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public decimal Amount { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; } = null!;
}