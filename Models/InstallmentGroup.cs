namespace QuantIA.Models;

public class InstallmentGroup
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int AccountId { get; set; }

    public int TotalInstallments { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
}