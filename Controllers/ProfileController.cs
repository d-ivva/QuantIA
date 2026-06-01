using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuantIA.DTOs;
using QuantIA.Interface;
using QuantIA.Services;

namespace QuantIA.Controllers;

[Authorize]
[Route("api/[controller]")]
public class ProfileController : AuthenticatedControllerBase
{
    private readonly ProfileService _profileService;

    public ProfileController(ProfileService profileService, ICurrentUserService currentUserService)
        : base(currentUserService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            var userId  = await GetCurrentUserIdAsync();
            var profile = await _profileService.GetProfileAsync(userId);
            if (profile == null) return NotFound(new { message = "Perfil não encontrado." });
            return Ok(profile);
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateProfileDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "O nome não pode estar vazio." });

            var userId     = await GetCurrentUserIdAsync();
            var keycloakId = GetKeycloakId();
            var profile    = await _profileService.UpdateNameAsync(userId, keycloakId, dto.Name);
            return Ok(new { message = "Perfil atualizado com sucesso.", data = profile });
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
        try
        {
            var userId     = await GetCurrentUserIdAsync();
            var keycloakId = GetKeycloakId();
            await _profileService.DeleteAccountAsync(userId, keycloakId);
            return Ok(new { message = "Conta excluída com sucesso." });
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
    }
}
