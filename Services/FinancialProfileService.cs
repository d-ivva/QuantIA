using Microsoft.EntityFrameworkCore;
using QuantIA.Data;
using QuantIA.Interface;
using QuantIA.Models;

namespace QuantIA.Services;

public class FinancialProfileService : IFinancialProfileService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public FinancialProfileService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<FinancialProfile> Criar(FinancialProfile request, int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new Exception("Perfil financeiro é obrigatório.");
        if (string.IsNullOrWhiteSpace(request.SalaryRange))
            throw new Exception("Faixa salarial é obrigatória.");

        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
            throw new Exception("Usuário não encontrado.");

        var alreadyExists = await _context.FinancialProfiles.AnyAsync(p => p.UserId == userId);
        if (alreadyExists)
            throw new Exception("Usuário já possui um perfil financeiro.");

        request.UserId    = userId;
        request.CreatedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;

        _context.FinancialProfiles.Add(request);
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task<FinancialProfile?> Buscar(int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.FinancialProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task Atualizar(FinancialProfile request, int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new Exception("Perfil financeiro é obrigatório.");
        if (string.IsNullOrWhiteSpace(request.SalaryRange))
            throw new Exception("Faixa salarial é obrigatória.");

        var profile = await _context.FinancialProfiles.FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new Exception("Perfil financeiro não encontrado.");

        profile.Name        = request.Name;
        profile.SalaryRange = request.SalaryRange;
        profile.UpdatedAt   = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task Deletar(int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();

        var profile = await _context.FinancialProfiles.FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new Exception("Perfil financeiro não encontrado.");

        _context.FinancialProfiles.Remove(profile);
        await _context.SaveChangesAsync();
    }
}
