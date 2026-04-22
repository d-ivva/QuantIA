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
            throw new ApplicationException("Valor deve ser maior que zero.");

        var account = await _context.Accounts.FindAsync(request.AccountId)
                      ?? throw new ApplicationException("Conta inválida.");

        var type = await _context.TransactionTypes.FindAsync(request.TransactionTypeId)
                   ?? throw new ApplicationException("Tipo inválido.");

        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId);

        if (!categoryExists)
            throw new ApplicationException("Categoria obrigatória e inválida.");

        if (request.IsInstallment)
        {
            if (request.Direction != TransactionDirection.expense)
                throw new ApplicationException("Apenas despesas podem ser parceladas.");

            if (!account.HasCreditCard)
                throw new ApplicationException("Conta não permite parcelamento.");

            if (request.InstallmentTotal == null || request.InstallmentTotal < 2)
                throw new ApplicationException("Parcelamento mínimo de 2x.");

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
                    TransactionDate = DateTime.SpecifyKind(
                        request.TransactionDate.AddMonths(i - 1),
                        DateTimeKind.Utc
                    ),
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

            request.TransactionDate = DateTime.SpecifyKind(
                request.TransactionDate,
                DateTimeKind.Utc
            );

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
            .OrderBy(t => t.TransactionDate)
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
                          ?? throw new ApplicationException("Transação não encontrada.");

        if (request.Amount <= 0)
            throw new ApplicationException("Valor deve ser maior que zero.");

        var account = await _context.Accounts.FindAsync(request.AccountId)
                      ?? throw new ApplicationException("Conta inválida.");

        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId);

        if (!categoryExists)
            throw new ApplicationException("Categoria inválida.");

        var typeExists = await _context.TransactionTypes
            .AnyAsync(t => t.Id == request.TransactionTypeId);

        if (!typeExists)
            throw new ApplicationException("Tipo inválido.");

        // 🔴 CASO: já é parcelado → atualiza grupo
        if (transaction.InstallmentGroupId != null)
        {
            if (request.IsInstallment != transaction.IsInstallment)
                throw new ApplicationException("Não é permitido alterar tipo de parcelamento.");

            if (request.InstallmentTotal != transaction.InstallmentTotal)
                throw new ApplicationException("Não é permitido alterar número de parcelas.");

            if (request.TransactionDate.Date != transaction.TransactionDate.Date)
                throw new ApplicationException("Não é permitido alterar a data de parcelas.");

            var groupId = transaction.InstallmentGroupId.Value;

            var transactions = await _context.Transactions
                .Where(t => t.InstallmentGroupId == groupId)
                .OrderBy(t => t.InstallmentNumber)
                .ToListAsync();

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
            // 🟢 CASO: virar parcelado
            if (request.IsInstallment)
            {
                if (request.Direction != TransactionDirection.expense)
                    throw new ApplicationException("Apenas despesas podem ser parceladas.");

                if (!account.HasCreditCard)
                    throw new ApplicationException("Conta não permite parcelamento.");

                if (request.InstallmentTotal == null || request.InstallmentTotal < 2)
                    throw new ApplicationException("Parcelamento mínimo de 2x.");

                _context.Transactions.Remove(transaction);

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
                        TransactionDate = DateTime.SpecifyKind(
                            request.TransactionDate.AddMonths(i - 1),
                            DateTimeKind.Utc
                        ),
                        IsInstallment = true,
                        InstallmentNumber = i,
                        InstallmentTotal = request.InstallmentTotal,
                        InstallmentGroupId = group.Id
                    });
                }
            }
            else
            {
                // 🔵 normal update
                transaction.AccountId = request.AccountId;
                transaction.CategoryId = request.CategoryId;
                transaction.TransactionTypeId = request.TransactionTypeId;
                transaction.Amount = request.Amount;
                transaction.Description = request.Description;
                transaction.TransactionDate = DateTime.SpecifyKind(
                    request.TransactionDate,
                    DateTimeKind.Utc
                );
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task Deletar(int id)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        var transaction = await _context.Transactions.FindAsync(id)
                          ?? throw new ApplicationException("Transação não encontrada.");

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