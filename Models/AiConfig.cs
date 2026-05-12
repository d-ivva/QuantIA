namespace QuantIA.Models;

public class AiConfig
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public AiProvider Provider { get; set; }
    public string ApiKey { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User? User { get; set; }
}

public enum AiProvider
{
    Claude,
    Gemini
}
