using Microsoft.EntityFrameworkCore;
using QuantIA.Data;
using QuantIA.DTOs;
using QuantIA.Interface;
using QuantIA.Models;

namespace QuantIA.Services;

public class MonthlyBudgetService : IMonthlyBudgetService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public MonthlyBudgetService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<MonthlyBudget> Criar(MonthlyBudget request)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        if (request.Amount <= 0)
            throw new ApplicationException("O valor da meta deve ser maior que zero.");

        if (request.Month < 1 || request.Month > 12)
            throw new ApplicationException("Mês inválido.");

        if (request.Year < 2000)
            throw new ApplicationException("Ano inválido.");

        var existe = await _context.MonthlyBudgets
            .AnyAsync(b => b.UserId == request.UserId && b.Month == request.Month && b.Year == request.Year);

        if (existe)
            throw new ApplicationException("Já existe uma meta para este mês e ano.");

        _context.MonthlyBudgets.Add(request);
        await _context.SaveChangesAsync();

        return request;
    }

    public async Task<List<MonthlyBudget>> Listar()
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        return await _context.MonthlyBudgets
            .Include(b => b.User)
            .OrderBy(b => b.Year)
            .ThenBy(b => b.Month)
            .ToListAsync();
    }

    public async Task<MonthlyBudget?> BuscarPorId(int id)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        return await _context.MonthlyBudgets
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<MonthlyBudget?> BuscarPorMesAno(int month, int year)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        return await _context.MonthlyBudgets
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Month == month && b.Year == year);
    }

    public async Task Atualizar(int id, MonthlyBudget request)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        var budget = await _context.MonthlyBudgets.FindAsync(id)
            ?? throw new ApplicationException("Meta não encontrada.");

        if (request.Amount <= 0)
            throw new ApplicationException("O valor da meta deve ser maior que zero.");

        if (request.Month < 1 || request.Month > 12)
            throw new ApplicationException("Mês inválido.");

        if (request.Year < 2000)
            throw new ApplicationException("Ano inválido.");

        var conflito = await _context.MonthlyBudgets
            .AnyAsync(b => b.Id != id && b.UserId == request.UserId && b.Month == request.Month && b.Year == request.Year);

        if (conflito)
            throw new ApplicationException("Já existe uma meta para este mês e ano.");

        budget.Amount = request.Amount;
        budget.Month = request.Month;
        budget.Year = request.Year;
        budget.UserId = request.UserId;

        await _context.SaveChangesAsync();
    }

    public async Task Deletar(int id)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        var budget = await _context.MonthlyBudgets.FindAsync(id)
            ?? throw new ApplicationException("Meta não encontrada.");

        _context.MonthlyBudgets.Remove(budget);
        await _context.SaveChangesAsync();
    }

    public async Task<BudgetReportDto> GerarRelatorio(int month, int year)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        var budget = await _context.MonthlyBudgets
            .FirstOrDefaultAsync(b => b.Month == month && b.Year == year);

        var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1);

        var transactions = await _context.Transactions
            .Include(t => t.Category)
            .Where(t =>
                t.Direction == TransactionDirection.expense &&
                t.TransactionDate >= startDate &&
                t.TransactionDate < endDate)
            .ToListAsync();

        var spentAmount = transactions.Sum(t => t.Amount);

        var byCategory = transactions
            .GroupBy(t => t.Category?.Name ?? "Sem categoria")
            .Select(g => new CategorySpendingDto
            {
                CategoryName = g.Key,
                Amount = g.Sum(t => t.Amount),
                Percentage = spentAmount > 0
                    ? Math.Round((double)(g.Sum(t => t.Amount) / spentAmount * 100), 1)
                    : 0
            })
            .OrderByDescending(c => c.Amount)
            .ToList();

        var budgetAmount = budget?.Amount;
        var remaining = budgetAmount.HasValue ? budgetAmount.Value - spentAmount : 0;
        var percentage = budgetAmount.HasValue && budgetAmount.Value > 0
            ? Math.Round((double)(spentAmount / budgetAmount.Value * 100), 1)
            : 0;

        return new BudgetReportDto
        {
            HasBudget = budget != null,
            BudgetAmount = budgetAmount,
            SpentAmount = spentAmount,
            RemainingAmount = remaining,
            PercentageUsed = percentage,
            Month = month,
            Year = year,
            ByCategory = byCategory
        };
    }
}
