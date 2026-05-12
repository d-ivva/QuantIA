using QuantIA.Models;

namespace QuantIA.DTOs;

public class ChatRequestDto
{
    public AiProvider Provider { get; set; }
    public string SessionId { get; set; } = null!;
    public string Message { get; set; } = null!;
}

public class ChatResponseDto
{
    public string SessionId { get; set; } = null!;
    public AiProvider Provider { get; set; }
    public string Response { get; set; } = null!;
}
