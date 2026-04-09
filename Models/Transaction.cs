namespace QuantIA.Models;

public class Transaction
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int AccountId { get; set; }

    public int CategoryId { get; set; }

    public int TransactionTypeId { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }
    
    public string Direction { get; set; }

    public DateTime TransactionDate { get; set; }

    public bool IsInstallment { get; set; }

    public int? InstallmentNumber { get; set; }

    public int? InstallmentTotal { get; set; }

    public int? InstallmentGroupId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; } = null!;
    public Account? Account { get; set; } 
    public Category? Category { get; set; }
    public TransactionType? TransactionType { get; set; } 
}