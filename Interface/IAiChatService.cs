using QuantIA.DTOs;

namespace QuantIA.Interface;

public interface IAiChatService
{
    Task<ChatResponseDto> Conversar(ChatRequestDto request, int userId);
    Task<List<string>> ListarSessoes(int userId);
    Task<List<object>> BuscarHistorico(string sessionId, int userId);
    Task LimparHistorico(string sessionId, int userId);
}
