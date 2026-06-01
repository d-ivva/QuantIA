namespace QuantIA.Models;

public class TransactionType
{
    public int    Id        { get; set; }
    public int?   UserId    { get; set; }  // null = tipo global (sistema); userId = tipo do usuário
    public string Name      { get; set; } = null!;
    public TransactionDirection Direction { get; set; }

    public User?  User      { get; set; }
}
