using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuantIA.Data;
using QuantIA.Models;

namespace QuantIA.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionTypesController : ControllerBase
{
    private readonly AppDbContext _context;

    public TransactionTypesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await _context.TransactionTypes.ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(TransactionType transactionType)
    {
        _context.TransactionTypes.Add(transactionType);
        await _context.SaveChangesAsync();
        return Ok(transactionType);
    }
}