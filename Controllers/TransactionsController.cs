using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuantIA.Data;
using QuantIA.Models;

namespace QuantIA.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly AppDbContext _context;

    public TransactionsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create(Transaction request)
    {
        var account = await _context.Accounts.FindAsync(request.AccountId);
        if (account == null) return BadRequest("Conta inválida");

        if (request.IsInstallment)
        {
            if (!account.HasCreditCard || account.CreditCardClosingDay == null)
                return BadRequest("Conta não permite parcelamento");
            
            await _context.SaveChangesAsync();
            
            for (int i = 1; i <= request.InstallmentTotal; i++)
            {
                _context.Transactions.Add(new Transaction
                {
                    AccountId = request.AccountId,
                    CategoryId = request.CategoryId,
                    TransactionTypeId = request.TransactionTypeId,
                    TransactionDate = request.TransactionDate.AddMonths(i - 1),
                    IsInstallment = true,
                    InstallmentNumber = i,
                    InstallmentTotal = request.InstallmentTotal,
                });
            }
        }
        else
        {
            _context.Transactions.Add(request);
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var data = await _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .ToListAsync();
        return Ok(data);
    }
}