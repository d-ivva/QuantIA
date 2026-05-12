using Microsoft.AspNetCore.Mvc;
using QuantIA.DTOs;
using QuantIA.Interface;

namespace QuantIA.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiConfigController : ControllerBase
{
    private readonly IAiConfigService _service;

    public AiConfigController(IAiConfigService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(AiConfigCreateDto request)
    {
        try
        {
            var result = await _service.Criar(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            return Ok(await _service.Listar());
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("tem-configuracao")]
    public async Task<IActionResult> TemConfiguracao()
    {
        try
        {
            var result = await _service.TemConfiguracao();
            return Ok(new { hasConfig = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, AiConfigCreateDto request)
    {
        try
        {
            await _service.Atualizar(id, request);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.Deletar(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
