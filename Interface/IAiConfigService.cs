using QuantIA.DTOs;

namespace QuantIA.Interface;

public interface IAiConfigService
{
    Task<AiConfigResponseDto> Criar(AiConfigCreateDto request, int userId);
    Task<List<AiConfigResponseDto>> Listar(int userId);
    Task Atualizar(int id, AiConfigCreateDto request, int userId);
    Task Deletar(int id, int userId);
    Task<bool> TemConfiguracao(int userId);
}
