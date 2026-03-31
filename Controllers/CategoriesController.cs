using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuantIA.Data;
using QuantIA.Models;

namespace QuantIA.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await _context.Categories.ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return Ok(category);
    }
}