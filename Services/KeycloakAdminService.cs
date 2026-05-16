using System.Net.Http.Headers;
using System.Text.Json;

namespace QuantIA.Services;

public class KeycloakAdminService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    private string AdminUrl     => _config["Keycloak:AdminUrl"]      ?? "http://localhost:8180";
    private string AdminRealm   => _config["Keycloak:AdminRealm"]    ?? "master";
    private string AdminClient  => _config["Keycloak:AdminClientId"] ?? "admin-cli";
    private string AdminUser    => _config["Keycloak:AdminUsername"]  ?? "admin";
    private string AdminPass    => _config["Keycloak:AdminPassword"]  ?? "admin123";
    private string Realm        => _config["Keycloak:Realm"]         ?? "quantia";

    public KeycloakAdminService(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    // ─── Admin token ──────────────────────────────────────────────────────
    private async Task<string> GetAdminTokenAsync()
    {
        var client = _httpClientFactory.CreateClient();
        var body = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"]  = AdminClient,
            ["username"]   = AdminUser,
            ["password"]   = AdminPass,
        });

        var res = await client.PostAsync($"{AdminUrl}/realms/{AdminRealm}/protocol/openid-connect/token", body);
        res.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("access_token").GetString()
            ?? throw new Exception("Token admin não retornado pelo Keycloak.");
    }

    private HttpClient AuthorizedClient(string token)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    // ─── Find user by email ───────────────────────────────────────────────
    public async Task<string?> FindUserIdByEmailAsync(string email)
    {
        var token  = await GetAdminTokenAsync();
        var client = AuthorizedClient(token);

        var res = await client.GetAsync(
            $"{AdminUrl}/admin/realms/{Realm}/users?email={Uri.EscapeDataString(email)}&exact=true");
        res.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        var users = doc.RootElement;
        if (users.GetArrayLength() == 0) return null;
        return users[0].GetProperty("id").GetString();
    }

    // ─── Send password-reset e-mail ───────────────────────────────────────
    public async Task SendPasswordResetEmailAsync(string userId)
    {
        var token  = await GetAdminTokenAsync();
        var client = AuthorizedClient(token);

        var actions = new[] { "UPDATE_PASSWORD" };
        var res = await client.PutAsJsonAsync(
            $"{AdminUrl}/admin/realms/{Realm}/users/{userId}/execute-actions-email", actions);

        res.EnsureSuccessStatusCode();
    }

    // ─── Create user ──────────────────────────────────────────────────────
    public async Task CreateUserAsync(string name, string email, string password)
    {
        var token  = await GetAdminTokenAsync();
        var client = AuthorizedClient(token);

        // Split name into first/last
        var parts     = name.Trim().Split(' ', 2);
        var firstName = parts[0];
        var lastName  = parts.Length > 1 ? parts[1] : "";

        var payload = new
        {
            username      = email,
            email         = email,
            firstName,
            lastName,
            enabled       = true,
            emailVerified = true,
            credentials   = new[] { new { type = "password", value = password, temporary = false } }
        };

        var res = await client.PostAsJsonAsync($"{AdminUrl}/admin/realms/{Realm}/users", payload);

        if (!res.IsSuccessStatusCode)
        {
            var body = await res.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(body);
                var msg = doc.RootElement.TryGetProperty("errorMessage", out var em)
                    ? em.GetString()
                    : null;
                throw new ApplicationException(msg ?? "Erro ao criar usuário.");
            }
            catch (JsonException)
            {
                throw new ApplicationException("Erro ao criar usuário.");
            }
        }
    }

    // ─── Update user name ─────────────────────────────────────────────────
    public async Task UpdateUserNameAsync(string keycloakUserId, string name)
    {
        var token  = await GetAdminTokenAsync();
        var client = AuthorizedClient(token);

        var parts     = name.Trim().Split(' ', 2);
        var firstName = parts[0];
        var lastName  = parts.Length > 1 ? parts[1] : "";

        var payload = new { firstName, lastName };
        await client.PutAsJsonAsync(
            $"{AdminUrl}/admin/realms/{Realm}/users/{keycloakUserId}", payload);
    }

    // ─── Delete user ──────────────────────────────────────────────────────
    public async Task DeleteUserAsync(string keycloakUserId)
    {
        var token  = await GetAdminTokenAsync();
        var client = AuthorizedClient(token);

        // keycloakUserId is the JWT `sub` claim, which is the Keycloak user UUID
        var res = await client.DeleteAsync(
            $"{AdminUrl}/admin/realms/{Realm}/users/{keycloakUserId}");

        res.EnsureSuccessStatusCode();
    }
}
