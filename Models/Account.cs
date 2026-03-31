namespace QuantIA.Models;

public class Account
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string Name { get; set; } = null!;

    public string? AccountNumber { get; set; }

    public string? BranchNumber { get; set; }

    public string Color { get; set; } = null!;

    public bool HasCreditCard { get; set; }

    public int? CreditCardClosingDay { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; } = null!;
}