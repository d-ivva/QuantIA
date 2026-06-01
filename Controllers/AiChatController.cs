using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuantIA.DTOs;
using QuantIA.Interface;

namespace QuantIA.Controllers;

[Authorize]
[Route("api/[controller]")]
public class AiChatController : AuthenticatedControllerBase
{
    private readonly IAiChatService _service;

    public AiChatController(IAiChatService service, ICurrentUserService currentUserService)
        : base(currentUserService)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Chat(ChatRequestDto request)
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            return Ok(await _service.Conversar(request, userId));
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("sessoes")]
    public async Task<IActionResult> GetSessoes()
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            return Ok(await _service.ListarSessoes(userId));
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("historico/{sessionId}")]
    public async Task<IActionResult> GetHistorico(string sessionId)
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            return Ok(await _service.BuscarHistorico(sessionId, userId));
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("historico/{sessionId}")]
    public async Task<IActionResult> LimparHistorico(string sessionId)
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            await _service.LimparHistorico(sessionId, userId);
            return Ok(new { message = "Histórico limpo com sucesso." });
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }
}
