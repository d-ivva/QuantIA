using Microsoft.EntityFrameworkCore;
using QuantIA.Data;
using QuantIA.Interface;
using QuantIA.Models;

namespace QuantIA.Services;

public class AccountService : IAccountService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public AccountService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Account> Criar(Account request)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();


        if (string.IsNullOrWhiteSpace(request.Name))
            throw new Exception("Nome da conta é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Color))
            throw new Exception("Cor da conta é obrigatória.");

        if (request.HasCreditCard)
        {
            if (request.CreditCardClosingDay == null ||
                request.CreditCardClosingDay < 1 ||
                request.CreditCardClosingDay > 31)
            {
                throw new Exception("Dia de fechamento do cartão inválido.");
            }
        }
        else
        {
            request.CreditCardClosingDay = null;
        }

        _context.Accounts.Add(request);
        await _context.SaveChangesAsync();

        return request;
    }

    public async Task<List<Account>> Listar()
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        return await _context.Accounts.ToListAsync();
    }

    public async Task<Account?> BuscarPorId(int id)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task Atualizar(int id, Account request)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        var account = await _context.Accounts.FindAsync(id)
            ?? throw new Exception("Conta não encontrada.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new Exception("Nome da conta é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Color))
            throw new Exception("Cor da conta é obrigatória.");

        if (request.HasCreditCard)
        {
            if (request.CreditCardClosingDay == null ||
                request.CreditCardClosingDay < 1 ||
                request.CreditCardClosingDay > 31)
            {
                throw new Exception("Dia de fechamento inválido.");
            }
        }
        else
        {
            request.CreditCardClosingDay = null;
        }

        account.Name = request.Name;
        account.AccountNumber = request.AccountNumber;
        account.BranchNumber = request.BranchNumber;
        account.Color = request.Color;
        account.HasCreditCard = request.HasCreditCard;
        account.CreditCardClosingDay = request.CreditCardClosingDay;

        await _context.SaveChangesAsync();
    }

    public async Task Deletar(int id)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        var account = await _context.Accounts.FindAsync(id)
            ?? throw new Exception("Conta não encontrada.");
        
        var hasTransactions = await _context.Transactions
            .AnyAsync(t => t.AccountId == id);

        if (hasTransactions)
            throw new Exception("Não é possível deletar conta com transações.");

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();
    }
}