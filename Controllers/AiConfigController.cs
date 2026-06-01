using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuantIA.DTOs;
using QuantIA.Interface;

namespace QuantIA.Controllers;

[Authorize]
[Route("api/[controller]")]
public class AiConfigController : AuthenticatedControllerBase
{
    private readonly IAiConfigService _service;

    public AiConfigController(IAiConfigService service, ICurrentUserService currentUserService)
        : base(currentUserService)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(AiConfigCreateDto request)
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            var result = await _service.Criar(request, userId);
            return Ok(new { message = "Configuração de IA criada com sucesso.", data = result });
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            return Ok(await _service.Listar(userId));
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("tem-configuracao")]
    public async Task<IActionResult> TemConfiguracao()
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            return Ok(new { hasConfig = await _service.TemConfiguracao(userId) });
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, AiConfigCreateDto request)
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            await _service.Atualizar(id, request, userId);
            return Ok(new { message = "Configuração de IA atualizada com sucesso." });
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            await _service.Deletar(id, userId);
            return Ok(new { message = "Configuração de IA removida com sucesso." });
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }
}
