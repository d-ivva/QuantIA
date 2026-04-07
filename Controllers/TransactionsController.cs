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
        if (account == null)
            return BadRequest("Conta inválida");
        
        if (request.IsInstallment)
        {
            if (!account.HasCreditCard || account.CreditCardClosingDay == null)
                return BadRequest("Conta não permite parcelamento");

            if (request.InstallmentTotal == null || request.InstallmentTotal <= 0)
                return BadRequest("Total de parcelas inválido");
            
            var groupId = Guid.NewGuid().GetHashCode();

            var installmentValue = request.Amount / request.InstallmentTotal.Value;

            for (int i = 1; i <= request.InstallmentTotal; i++)
            {
                var transaction = new Transaction
                {
                    AccountId = request.AccountId,
                    CategoryId = request.CategoryId,
                    TransactionTypeId = request.TransactionTypeId,
                    Amount = installmentValue,
                    Description = request.Description,
                    TransactionDate = request.TransactionDate.AddMonths(i - 1),

                    IsInstallment = true,
                    InstallmentNumber = i,
                    InstallmentTotal = request.InstallmentTotal,
                    InstallmentGroupId = groupId
                };

                _context.Transactions.Add(transaction);
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
            .Include(t => t.TransactionType)
            .ToListAsync();

        return Ok(data);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .Include(t => t.TransactionType)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transaction == null)
            return NotFound();

        return Ok(transaction);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Transaction request)
    {
        var transaction = await _context.Transactions.FindAsync(id);

        if (transaction == null)
            return NotFound();

        transaction.AccountId = request.AccountId;
        transaction.CategoryId = request.CategoryId;
        transaction.TransactionTypeId = request.TransactionTypeId;
        transaction.Amount = request.Amount;
        transaction.Description = request.Description;
        transaction.TransactionDate = request.TransactionDate;

        await _context.SaveChangesAsync();

        return Ok(transaction);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);

        if (transaction == null)
            return NotFound();

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();

        return Ok();
    }
}