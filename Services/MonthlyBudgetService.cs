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

    public async Task<MonthlyBudget> Criar(MonthlyBudget request, int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();

        if (request.Amount <= 0) throw new ApplicationException("O valor da meta deve ser maior que zero.");
        if (request.Month < 1 || request.Month > 12) throw new ApplicationException("Mês inválido.");
        if (request.Year < 2000) throw new ApplicationException("Ano inválido.");

        var existe = await _context.MonthlyBudgets.AnyAsync(b => b.UserId == userId && b.Month == request.Month && b.Year == request.Year);
        if (existe) throw new ApplicationException("Já existe uma meta para este mês e ano.");

        request.UserId = userId;
        _context.MonthlyBudgets.Add(request);
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task<List<MonthlyBudget>> Listar(int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.MonthlyBudgets.Where(b => b.UserId == userId).OrderBy(b => b.Year).ThenBy(b => b.Month).ToListAsync();
    }

    public async Task<MonthlyBudget?> BuscarPorId(int id, int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.MonthlyBudgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
    }

    public async Task<MonthlyBudget?> BuscarPorMesAno(int month, int year, int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.MonthlyBudgets.FirstOrDefaultAsync(b => b.Month == month && b.Year == year && b.UserId == userId);
    }

    public async Task Atualizar(int id, MonthlyBudget request, int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();

        var budget = await _context.MonthlyBudgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId)
            ?? throw new ApplicationException("Meta não encontrada.");

        if (request.Amount <= 0) throw new ApplicationException("O valor da meta deve ser maior que zero.");
        if (request.Month < 1 || request.Month > 12) throw new ApplicationException("Mês inválido.");
        if (request.Year < 2000) throw new ApplicationException("Ano inválido.");

        var conflito = await _context.MonthlyBudgets.AnyAsync(b => b.Id != id && b.UserId == userId && b.Month == request.Month && b.Year == request.Year);
        if (conflito) throw new ApplicationException("Já existe uma meta para este mês e ano.");

        budget.Amount = request.Amount;
        budget.Month = request.Month;
        budget.Year = request.Year;
        await _context.SaveChangesAsync();
    }

    public async Task Deletar(int id, int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();

        var budget = await _context.MonthlyBudgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId)
            ?? throw new ApplicationException("Meta não encontrada.");

        _context.MonthlyBudgets.Remove(budget);
        await _context.SaveChangesAsync();
    }

    public async Task<BudgetReportDto> GerarRelatorio(int month, int year, int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();

        var budget = await _context.MonthlyBudgets.FirstOrDefaultAsync(b => b.Month == month && b.Year == year && b.UserId == userId);
        var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1);

        var transactions = await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.Direction == TransactionDirection.expense && t.TransactionDate >= startDate && t.TransactionDate < endDate)
            .ToListAsync();

        var spentAmount = transactions.Sum(t => t.Amount);
        var byCategory = transactions
            .GroupBy(t => t.Category?.Name ?? "Sem categoria")
            .Select(g => new CategorySpendingDto
            {
                CategoryName = g.Key, Amount = g.Sum(t => t.Amount),
                Percentage = spentAmount > 0 ? Math.Round((double)(g.Sum(t => t.Amount) / spentAmount * 100), 1) : 0
            })
            .OrderByDescending(c => c.Amount).ToList();

        var budgetAmount = budget?.Amount;
        var remaining = budgetAmount.HasValue ? budgetAmount.Value - spentAmount : 0;
        var percentage = budgetAmount.HasValue && budgetAmount.Value > 0 ? Math.Round((double)(spentAmount / budgetAmount.Value * 100), 1) : 0;

        return new BudgetReportDto { HasBudget = budget != null, BudgetAmount = budgetAmount, SpentAmount = spentAmount, RemainingAmount = remaining, PercentageUsed = percentage, Month = month, Year = year, ByCategory = byCategory };
    }

    public async Task<object> GetDashboardData(int month, int year, int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();

        var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1);

        var transactions = await _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .Where(t => t.UserId == userId && t.TransactionDate >= startDate && t.TransactionDate < endDate)
            .ToListAsync();

        var totalIncome = transactions.Where(t => t.Direction == TransactionDirection.income).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.Direction == TransactionDirection.expense).Sum(t => t.Amount);
        var netBalance = totalIncome - totalExpense;

        var mostMovedAccount = transactions
            .GroupBy(t => t.Account?.Name)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault() ?? "Nenhuma";

        var daysInMonth = DateTime.DaysInMonth(year, month);
        var dailyFlow = new List<object>();
        decimal runningBalance = 0;

        for (int day = 1; day <= daysInMonth; day++)
        {
            var dayTransactions = transactions.Where(t => t.TransactionDate.Day == day).ToList();
            var income = dayTransactions.Where(t => t.Direction == TransactionDirection.income).Sum(t => t.Amount);
            var expense = dayTransactions.Where(t => t.Direction == TransactionDirection.expense).Sum(t => t.Amount);
            
            runningBalance += (income - expense);

            dailyFlow.Add(new 
            { 
                Day = day, 
                Income = income, 
                Expense = expense,
                Balance = runningBalance
            });
        }

        var byCategory = transactions
            .Where(t => t.Direction == TransactionDirection.expense)
            .GroupBy(t => new { Name = t.Category?.Name ?? "Sem categoria", Color = t.Category?.Color ?? "#64748b" })
            .Select(g => new
            {
                CategoryName = g.Key.Name,
                Color = g.Key.Color,
                Amount = g.Sum(t => t.Amount),
                Percentage = totalExpense > 0 ? Math.Round((double)(g.Sum(t => t.Amount) / totalExpense * 100), 1) : 0
            })
            .OrderByDescending(c => c.Amount)
            .ToList();

        var budget = await _context.MonthlyBudgets.FirstOrDefaultAsync(b => b.UserId == userId && b.Month == month && b.Year == year);

        return new
        {
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            NetBalance = netBalance,
            MostMovedAccount = mostMovedAccount,
            DailyFlow = dailyFlow,
            ByCategory = byCategory,
            BudgetAmount = budget?.Amount ?? 0
        };
    }

    public async Task<object> GetAnnualReport(int year, int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();

        var startDate = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(year + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId && t.TransactionDate >= startDate && t.TransactionDate < endDate)
            .ToListAsync();

        var budgets = await _context.MonthlyBudgets
            .Where(b => b.UserId == userId && b.Year == year)
            .ToListAsync();

        var monthsData = Enumerable.Range(1, 12).Select(month =>
        {
            var monthTransactions = transactions.Where(t => t.TransactionDate.Month == month).ToList();
            var income = monthTransactions.Where(t => t.Direction == TransactionDirection.income).Sum(t => t.Amount);
            var expense = monthTransactions.Where(t => t.Direction == TransactionDirection.expense).Sum(t => t.Amount);
            var budget = budgets.FirstOrDefault(b => b.Month == month)?.Amount ?? 0;

            var netBalance = income - expense;
            var differenceToBudget = budget > 0 ? budget - expense : netBalance;

            var monthName = new DateTime(year, month, 1).ToString("MMM", new System.Globalization.CultureInfo("pt-BR"));

            return new
            {
                Month = month,
                MonthName = monthName.ToUpper().Replace(".", ""),
                Balance = netBalance,
                Performance = differenceToBudget
            };
        }).ToList();

        return monthsData;
    }
}