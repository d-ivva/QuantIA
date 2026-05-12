using QuantIA.DTOs;

namespace QuantIA.Interface;

public interface IAiChatService
{
    Task<ChatResponseDto> Conversar(ChatRequestDto request);
    Task<List<string>> ListarSessoes();
    Task<List<object>> BuscarHistorico(string sessionId);
    Task LimparHistorico(string sessionId);
}
