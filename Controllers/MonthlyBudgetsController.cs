using Microsoft.AspNetCore.Mvc;
using QuantIA.Data;
using QuantIA.Models;

namespace QuantIA.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MonthlyBudgetsController : ControllerBase
{
    private readonly AppDbContext _context;

    public MonthlyBudgetsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create(MonthlyBudget budget)
    {
        _context.MonthlyBudgets.Add(budget);
        await _context.SaveChangesAsync();
        return Ok(budget);
    }
}