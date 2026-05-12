using Microsoft.AspNetCore.Mvc;
using QuantIA.DTOs;
using QuantIA.Interface;

namespace QuantIA.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiChatController : ControllerBase
{
    private readonly IAiChatService _service;

    public AiChatController(IAiChatService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Chat(ChatRequestDto request)
    {
        try
        {
            var result = await _service.Conversar(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("sessoes")]
    public async Task<IActionResult> GetSessoes()
    {
        try
        {
            return Ok(await _service.ListarSessoes());
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("historico/{sessionId}")]
    public async Task<IActionResult> GetHistorico(string sessionId)
    {
        try
        {
            return Ok(await _service.BuscarHistorico(sessionId));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("historico/{sessionId}")]
    public async Task<IActionResult> LimparHistorico(string sessionId)
    {
        try
        {
            await _service.LimparHistorico(sessionId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
