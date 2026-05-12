using QuantIA.DTOs;

namespace QuantIA.Interface;

public interface IAiConfigService
{
    Task<AiConfigResponseDto> Criar(AiConfigCreateDto request);
    Task<List<AiConfigResponseDto>> Listar();
    Task Atualizar(int id, AiConfigCreateDto request);
    Task Deletar(int id);
    Task<bool> TemConfiguracao();
}
