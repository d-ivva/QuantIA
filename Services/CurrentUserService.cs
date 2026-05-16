using Microsoft.EntityFrameworkCore;
using QuantIA.Data;
using QuantIA.Interface;
using QuantIA.Models;

namespace QuantIA.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private int? _cachedUserId;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        IDbContextFactory<AppDbContext> contextFactory)
    {
        _httpContextAccessor = httpContextAccessor;
        _contextFactory = contextFactory;
    }

    public async Task<int> GetUserIdAsync()
    {
        if (_cachedUserId.HasValue)
            return _cachedUserId.Value;

        var keycloakId = _httpContextAccessor.HttpContext?.User
            .FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("Token inválido ou ausente.");

        await using var context = _contextFactory.CreateDbContext();

        var user = await context.Users.FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);

        if (user == null)
        {
            var claims = _httpContextAccessor.HttpContext!.User.Claims;
            user = new User
            {
                KeycloakId = keycloakId,
                Email      = claims.FirstOrDefault(c => c.Type == "email")?.Value ?? string.Empty,
                Name       = claims.FirstOrDefault(c => c.Type == "name")?.Value
                             ?? claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value
                             ?? keycloakId,
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        _cachedUserId = user.Id;
        return user.Id;
    }

    public string GetKeycloakId() =>
        _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value
        ?? throw new UnauthorizedAccessException("Token inválido ou ausente.");
}
