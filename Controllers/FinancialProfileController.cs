using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuantIA.Interface;
using QuantIA.Models;

namespace QuantIA.Controllers;

[Authorize]
[Route("api/[controller]")]
public class FinancialProfileController : AuthenticatedControllerBase
{
    private readonly IFinancialProfileService _service;

    public FinancialProfileController(IFinancialProfileService service, ICurrentUserService currentUserService)
        : base(currentUserService)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(FinancialProfile request)
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            var result = await _service.Criar(request, userId);
            return Ok(new { message = "Perfil financeiro criado com sucesso.", data = result });
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
            var data   = await _service.Buscar(userId);
            if (data == null) return NotFound(new { message = "Perfil financeiro não encontrado." });
            return Ok(data);
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut]
    public async Task<IActionResult> Update(FinancialProfile request)
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            await _service.Atualizar(request, userId);
            return Ok(new { message = "Perfil financeiro atualizado com sucesso." });
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            await _service.Deletar(userId);
            return Ok(new { message = "Perfil financeiro excluído com sucesso." });
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }
}
