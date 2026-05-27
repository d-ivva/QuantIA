using Microsoft.AspNetCore.Mvc;
using QuantIA.Interface;

namespace QuantIA.Controllers;

[ApiController]
public abstract class AuthenticatedControllerBase : ControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    protected AuthenticatedControllerBase(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    protected Task<int> GetCurrentUserIdAsync() => _currentUserService.GetUserIdAsync();

    protected string GetKeycloakId() => _currentUserService.GetKeycloakId();
}
