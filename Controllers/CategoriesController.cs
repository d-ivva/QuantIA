using Microsoft.AspNetCore.Mvc;
using QuantIA.Models;
using QuantIA.Services;

namespace QuantIA.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service)
    {
        _service = service;
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
        
        if (data == null) 
            return NotFound(new { mensagem = $"Categoria {id} não encontrada." });

        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Category category)
    {
        try
        {
            var result = await _service.Criar(category);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Category category)
    {
        if (id != category.Id)
            return BadRequest(new { mensagem = "IDs não coincidem." });

        try
        {
            await _service.Atualizar(id, category);
            return NoContent();
        }
        catch (Exception ex)
        {
            if (ex.Message == "Categoria não encontrada.") 
                return NotFound();
                
            return BadRequest(new { mensagem = ex.Message });
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
            if (ex.Message == "Categoria não encontrada.") 
                return NotFound();
                
            return BadRequest(new { mensagem = ex.Message }); 
        }
    }
}