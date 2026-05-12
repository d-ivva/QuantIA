namespace QuantIA.Models;

public class AiMessage
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string SessionId { get; set; } = null!;
    public AiProvider Provider { get; set; }
    public string Role { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User? User { get; set; }
}
