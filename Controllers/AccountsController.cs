using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuantIA.Data;
using QuantIA.Models;

namespace QuantIA.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AccountsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await _context.Accounts.ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Account account)
    {
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        return Ok(account);
    }
}