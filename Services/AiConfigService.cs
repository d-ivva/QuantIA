using Microsoft.EntityFrameworkCore;
using QuantIA.Data;
using QuantIA.DTOs;
using QuantIA.Interface;
using QuantIA.Models;

namespace QuantIA.Services;

public class AiConfigService : IAiConfigService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public AiConfigService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<AiConfigResponseDto> Criar(AiConfigCreateDto request, int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();

        if (string.IsNullOrWhiteSpace(request.ApiKey)) throw new ApplicationException("A chave de API é obrigatória.");

        var existe = await _context.AiConfigs.AnyAsync(c => c.UserId == userId && c.Provider == request.Provider);
        if (existe) throw new ApplicationException($"Já existe uma configuração de {request.Provider}. Atualize a existente.");

        var config = new AiConfig { UserId = userId, Provider = request.Provider, ApiKey = request.ApiKey, IsActive = true };
        _context.AiConfigs.Add(config);
        await _context.SaveChangesAsync();
        return ToResponseDto(config);
    }

    public async Task<List<AiConfigResponseDto>> Listar(int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();
        var configs = await _context.AiConfigs.Where(c => c.UserId == userId).OrderBy(c => c.Provider).ToListAsync();
        return configs.Select(ToResponseDto).ToList();
    }

    public async Task Atualizar(int id, AiConfigCreateDto request, int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();

        var config = await _context.AiConfigs.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId)
            ?? throw new ApplicationException("Configuração não encontrada.");

        if (string.IsNullOrWhiteSpace(request.ApiKey)) throw new ApplicationException("A chave de API é obrigatória.");

        config.ApiKey = request.ApiKey;
        config.IsActive = true;
        await _context.SaveChangesAsync();
    }

    public async Task Deletar(int id, int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();

        var config = await _context.AiConfigs.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId)
            ?? throw new ApplicationException("Configuração não encontrada.");

        _context.AiConfigs.Remove(config);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> TemConfiguracao(int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.AiConfigs.AnyAsync(c => c.UserId == userId && c.IsActive);
    }

    private static AiConfigResponseDto ToResponseDto(AiConfig config) => new()
    {
        Id = config.Id, Provider = config.Provider,
        ApiKeyPreview = config.ApiKey.Length <= 8 ? "****" : $"****{config.ApiKey[^4..]}",
        IsActive = config.IsActive, CreatedAt = config.CreatedAt
    };
}
