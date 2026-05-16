using Microsoft.EntityFrameworkCore;
using QuantIA.Data;
using QuantIA.DTOs;

namespace QuantIA.Services;

public class ProfileService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly KeycloakAdminService _keycloakAdmin;

    public ProfileService(
        IDbContextFactory<AppDbContext> contextFactory,
        KeycloakAdminService keycloakAdmin)
    {
        _contextFactory = contextFactory;
        _keycloakAdmin  = keycloakAdmin;
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

        // Best effort: sync name to Keycloak
        try { await _keycloakAdmin.UpdateUserNameAsync(keycloakId, user.Name); }
        catch { /* non-critical */ }

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

        // Delete from Keycloak after DB is clean (best effort)
        try { await _keycloakAdmin.DeleteUserAsync(keycloakId); }
        catch { /* log in production */ }
    }
}
