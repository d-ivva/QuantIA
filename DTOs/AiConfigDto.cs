using QuantIA.Models;

namespace QuantIA.DTOs;

public class AiConfigCreateDto
{
    public AiProvider Provider { get; set; }
    public string ApiKey { get; set; } = null!;
}

public class AiConfigResponseDto
{
    public int Id { get; set; }
    public AiProvider Provider { get; set; }
    public string ApiKeyPreview { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
