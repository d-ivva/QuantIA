using Microsoft.EntityFrameworkCore;
using QuantIA.Data;
using QuantIA.Interface;
using QuantIA.Models;

namespace QuantIA.Services;

public class TransactionService : ITransactionService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public TransactionService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Transaction> Criar(Transaction request)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        if (request.Amount <= 0)
            throw new Exception("Valor deve ser maior que zero.");

        var account = await _context.Accounts.FindAsync(request.AccountId)
            ?? throw new Exception("Conta inválida.");

        var type = await _context.TransactionTypes.FindAsync(request.TransactionTypeId)
            ?? throw new Exception("Tipo inválido.");

        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId);

        if (!categoryExists)
            throw new Exception("Categoria obrigatória e inválida.");

        if (request.IsInstallment)
        {
            if (request.Direction != TransactionDirection.expense)
                throw new Exception("Apenas despesas podem ser parceladas.");

            if (!account.HasCreditCard)
                throw new Exception("Conta não permite parcelamento.");

            if (request.InstallmentTotal == null || request.InstallmentTotal < 2)
                throw new Exception("Parcelamento mínimo de 2x.");
            
            var group = new InstallmentGroup
            {
                AccountId = request.AccountId,
                TotalInstallments = request.InstallmentTotal.Value,
                TotalAmount = request.Amount
            };

            _context.InstallmentGroups.Add(group);
            await _context.SaveChangesAsync();

            var installmentValue = Math.Round(
                request.Amount / request.InstallmentTotal.Value, 2
            );

            for (int i = 1; i <= request.InstallmentTotal; i++)
            {
                _context.Transactions.Add(new Transaction
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
                    InstallmentGroupId = group.Id
                });
            }
        }
        else
        {
            request.InstallmentGroupId = null;
            request.InstallmentNumber = null;
            request.InstallmentTotal = null;

            _context.Transactions.Add(request);
        }

        await _context.SaveChangesAsync();
        return request;
    }

    public async Task<List<Transaction>> Listar()
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        return await _context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Include(t => t.TransactionType)
            .ToListAsync();
    }

    public async Task<Transaction?> BuscarPorId(int id)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        return await _context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Include(t => t.TransactionType)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task Atualizar(int id, Transaction request)
{
    using var _context = await _contextFactory.CreateDbContextAsync();

    var transaction = await _context.Transactions.FindAsync(id)
        ?? throw new Exception("Transação não encontrada.");

    if (request.Amount <= 0)
        throw new Exception("Valor deve ser maior que zero.");
    
    var account = await _context.Accounts.FindAsync(request.AccountId)
        ?? throw new Exception("Conta inválida.");
    
    var categoryExists = await _context.Categories
        .AnyAsync(c => c.Id == request.CategoryId);

    if (!categoryExists)
        throw new Exception("Categoria inválida.");
    
    var typeExists = await _context.TransactionTypes
        .AnyAsync(t => t.Id == request.TransactionTypeId);

    if (!typeExists)
        throw new Exception("Tipo inválido.");
    
    if (transaction.InstallmentGroupId != null)
    {
        if (request.IsInstallment != transaction.IsInstallment)
            throw new Exception("Não é permitido alterar tipo de parcelamento.");

        if (request.InstallmentTotal != transaction.InstallmentTotal)
            throw new Exception("Não é permitido alterar número de parcelas.");

        if (request.TransactionDate != transaction.TransactionDate)
            throw new Exception("Não é permitido alterar a data de parcelas.");

        var groupId = transaction.InstallmentGroupId.Value;

        var transactions = await _context.Transactions
            .Where(t => t.InstallmentGroupId == groupId)
            .OrderBy(t => t.InstallmentNumber)
            .ToListAsync();

        if (!transactions.Any())
            throw new Exception("Grupo de parcelas não encontrado.");

        var totalInstallments = transactions.First().InstallmentTotal ?? 1;

        var baseValue = Math.Floor((request.Amount / totalInstallments) * 100) / 100;
        var totalBase = baseValue * totalInstallments;
        var difference = request.Amount - totalBase;

        for (int i = 0; i < transactions.Count; i++)
        {
            var t = transactions[i];

            t.AccountId = request.AccountId;
            t.CategoryId = request.CategoryId;
            t.TransactionTypeId = request.TransactionTypeId;
            t.Description = request.Description;

            if (i == transactions.Count - 1)
                t.Amount = baseValue + difference;
            else
                t.Amount = baseValue;
        }
    }
    else
    {
        transaction.AccountId = request.AccountId;
        transaction.CategoryId = request.CategoryId;
        transaction.TransactionTypeId = request.TransactionTypeId;
        transaction.Amount = request.Amount;
        transaction.Description = request.Description;
        transaction.TransactionDate = request.TransactionDate;
    }

    await _context.SaveChangesAsync();
}

    public async Task Deletar(int id)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        var transaction = await _context.Transactions.FindAsync(id)
            ?? throw new Exception("Transação não encontrada.");

        if (transaction.InstallmentGroupId != null)
        {
            var groupId = transaction.InstallmentGroupId;

            var transactions = await _context.Transactions
                .Where(t => t.InstallmentGroupId == groupId)
                .ToListAsync();

            _context.Transactions.RemoveRange(transactions);

            var group = await _context.InstallmentGroups.FindAsync(groupId);
            if (group != null)
                _context.InstallmentGroups.Remove(group);
        }
        else
        {
            _context.Transactions.Remove(transaction);
        }

        await _context.SaveChangesAsync();
    }
}