using Microsoft.EntityFrameworkCore;
using QuantIA.Data;
using QuantIA.DTOs;

namespace QuantIA.Services;

public class ProfileService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly KeycloakAdminService _keycloakAdmin;
    private readonly ILogger<ProfileService> _logger;

    public ProfileService(
        IDbContextFactory<AppDbContext> contextFactory,
        KeycloakAdminService keycloakAdmin,
        ILogger<ProfileService> logger)
    {
        _contextFactory = contextFactory;
        _keycloakAdmin  = keycloakAdmin;
        _logger         = logger;
    }

    // ─── Get profile ──────────────────────────────────────────────────────
    public async Task<ProfileResponseDto?> GetProfileAsync(int userId)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        var user = await ctx.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return null;

        return new ProfileResponseDto
        {
            Id        = user.Id,
            Name      = user.Name,
            Email     = user.Email,
            CreatedAt = user.CreatedAt,
        };
    }

    // ─── Update name ──────────────────────────────────────────────────────
    public async Task<ProfileResponseDto> UpdateNameAsync(int userId, string keycloakId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ApplicationException("O nome não pode estar vazio.");

        await using var ctx = _contextFactory.CreateDbContext();
        var user = await ctx.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new ApplicationException("Usuário não encontrado.");

        user.Name = name.Trim();
        await ctx.SaveChangesAsync();

        // #4: Best effort — log failures so ops can detect Keycloak drift
        try { await _keycloakAdmin.UpdateUserNameAsync(keycloakId, user.Name); }
        catch (Exception ex) { _logger.LogWarning(ex, "Falha ao sincronizar nome no Keycloak para {KeycloakId}", keycloakId); }

        return new ProfileResponseDto
        {
            Id        = user.Id,
            Name      = user.Name,
            Email     = user.Email,
            CreatedAt = user.CreatedAt,
        };
    }

    // ─── Delete account (cascade) ─────────────────────────────────────────
    public async Task DeleteAccountAsync(int userId, string keycloakId)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        await using var tx  = await ctx.Database.BeginTransactionAsync();

        try
        {
            // Delete in FK-safe order
            await ctx.AiMessages        .Where(m => m.UserId == userId).ExecuteDeleteAsync();
            await ctx.AiConfigs         .Where(c => c.UserId == userId).ExecuteDeleteAsync();
            await ctx.Transactions      .Where(t => t.UserId == userId).ExecuteDeleteAsync();
            await ctx.InstallmentGroups .Where(g => g.UserId == userId).ExecuteDeleteAsync();
            await ctx.MonthlyBudgets    .Where(b => b.UserId == userId).ExecuteDeleteAsync();
            await ctx.Categories        .Where(c => c.UserId == userId).ExecuteDeleteAsync();
            await ctx.TransactionTypes  .Where(t => t.UserId == userId).ExecuteDeleteAsync();
            await ctx.Accounts          .Where(a => a.UserId == userId).ExecuteDeleteAsync();
            await ctx.Users             .Where(u => u.Id     == userId).ExecuteDeleteAsync();

            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        // #4: Delete from Keycloak after DB is clean — log failures so the orphan can be cleaned up
        try { await _keycloakAdmin.DeleteUserAsync(keycloakId); }
        catch (Exception ex) { _logger.LogError(ex, "Falha ao deletar usuário {KeycloakId} no Keycloak após remoção do DB", keycloakId); }
    }
}
