using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuantIA.Interface;
using QuantIA.Models;

namespace QuantIA.Controllers;

[Authorize]
[Route("api/[controller]")]
public class AccountsController : AuthenticatedControllerBase
{
    private readonly IAccountService _service;

    public AccountsController(IAccountService service, ICurrentUserService currentUserService)
        : base(currentUserService)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(Account request)
    {
        try { var userId = await GetCurrentUserIdAsync(); return Ok(await _service.Criar(request, userId)); }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try { var userId = await GetCurrentUserIdAsync(); return Ok(await _service.Listar(userId)); }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try { var userId = await GetCurrentUserIdAsync(); var data = await _service.BuscarPorId(id, userId); if (data == null) return NotFound(); return Ok(data); }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Account request)
    {
        try { var userId = await GetCurrentUserIdAsync(); await _service.Atualizar(id, request, userId); return NoContent(); }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { var userId = await GetCurrentUserIdAsync(); await _service.Deletar(id, userId); return NoContent(); }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }
}
