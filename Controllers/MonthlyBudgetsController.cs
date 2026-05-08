using Microsoft.AspNetCore.Mvc;
using QuantIA.Interface;
using QuantIA.Models;

namespace QuantIA.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MonthlyBudgetsController : ControllerBase
{
    private readonly IMonthlyBudgetService _service;

    public MonthlyBudgetsController(IMonthlyBudgetService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(MonthlyBudget request)
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var data = await _service.BuscarPorId(id);
            if (data == null) return NotFound();

            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{month}/{year}")]
    public async Task<IActionResult> GetByMesAno(int month, int year)
    {
        try
        {
            var data = await _service.BuscarPorMesAno(month, year);
            if (data == null) return NotFound();

            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, MonthlyBudget request)
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