using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuantIA.Data;
using QuantIA.Models;
using QuantIA.Services;

namespace QuantIA.Controllers;
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _service;

    public TransactionsController(ITransactionService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(Transaction request)
    {
        var result = await _service.Criar(request);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await _service.Listar());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var data = await _service.BuscarPorId(id);
        if (data == null) return NotFound();

        return Ok(data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Transaction request)
    {
        await _service.Atualizar(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.Deletar(id);
        return NoContent();
    }
}