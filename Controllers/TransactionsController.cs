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
        if (request.Amount <= 0)
            return BadRequest("O valor deve ser maior que zero.");

        var account = await _context.Accounts.FindAsync(request.AccountId);
        if (account == null)
            return BadRequest("Conta inválida.");

        var type = await _context.TransactionTypes.FindAsync(request.TransactionTypeId);
        if (type == null)
            return BadRequest("Tipo de transação inválido.");

        if (request.CategoryId != null)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == request.CategoryId);

            if (!categoryExists)
                return BadRequest("Categoria inválida.");
        }

        if (request.IsInstallment)
        {
            if (request.Direction != "expense")
                return BadRequest("Apenas despesas podem ser parceladas.");

            if (!account.HasCreditCard || account.CreditCardClosingDay == null)
                return BadRequest("Conta não permite parcelamento.");

            if (request.InstallmentTotal == null || request.InstallmentTotal < 2)
                return BadRequest("Número de parcelas inválido (mínimo 2).");

            var groupId = Guid.NewGuid().GetHashCode();
            var installmentValue = request.Amount / request.InstallmentTotal.Value;

            for (int i = 1; i <= request.InstallmentTotal; i++)
            {
                var transaction = new Transaction
                {
                    AccountId = request.AccountId,
                    CategoryId = request.CategoryId,
                    TransactionTypeId = request.TransactionTypeId,
                    Direction = request.Direction,
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
            request.IsInstallment = false;
            request.InstallmentNumber = null;
            request.InstallmentTotal = null;
            request.InstallmentGroupId = null;

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

        if (request.Amount <= 0)
            return BadRequest("Valor inválido.");

        var accountExists = await _context.Accounts
            .AnyAsync(a => a.Id == request.AccountId);

        if (!accountExists)
            return BadRequest("Conta inválida.");

        if (transaction.IsInstallment)
        {
            return BadRequest("Não é permitido editar transações parceladas individualmente.");
        }

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
        
        if (transaction.IsInstallment && transaction.InstallmentGroupId != null)
        {
            var group = await _context.Transactions
                .Where(t => t.InstallmentGroupId == transaction.InstallmentGroupId)
                .ToListAsync();

            _context.Transactions.RemoveRange(group);
        }
        else
        {
            _context.Transactions.Remove(transaction);
        }

        await _context.SaveChangesAsync();

        return Ok();
    }
}