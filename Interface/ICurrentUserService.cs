namespace QuantIA.Interface;

public interface ICurrentUserService
{
    Task<int> GetUserIdAsync();
    string GetKeycloakId();
}
