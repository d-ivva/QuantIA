using Microsoft.EntityFrameworkCore;
using QuantIA.Data;
using QuantIA.Interface;
using QuantIA.Models;

namespace QuantIA.Services;

public class CategoryService : ICategoryService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public CategoryService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Category> Criar(Category request, int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ApplicationException("O nome da categoria é obrigatório.");

        var name = request.Name.Trim();

        var exists = await _context.Categories
            .AnyAsync(c => c.UserId == userId && c.Name.ToLower() == name.ToLower());

        if (exists)
            throw new ApplicationException("Já existe uma categoria com esse nome.");

        request.Name = name;
        request.UserId = userId;

        _context.Categories.Add(request);
        await _context.SaveChangesAsync();

        return request;
    }

    public async Task<List<Category>> Listar(int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();

        return await _context.Categories
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> BuscarPorId(int id, int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();

        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
    }

    public async Task Atualizar(int id, Category request, int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId)
            ?? throw new ApplicationException("Categoria não encontrada.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ApplicationException("O nome da categoria é obrigatório.");

        var name = request.Name.Trim();

        var exists = await _context.Categories
            .AnyAsync(c => c.Id != id && c.UserId == userId && c.Name.ToLower() == name.ToLower());

        if (exists)
            throw new ApplicationException("Já existe outra categoria com esse nome.");

        category.Name = name;
        category.Color = string.IsNullOrWhiteSpace(request.Color) ? "#6B7280" : request.Color;

        await _context.SaveChangesAsync();
    }

    public async Task Deletar(int id, int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId)
            ?? throw new ApplicationException("Categoria não encontrada.");

        var hasTransactions = await _context.Transactions
            .AnyAsync(t => t.CategoryId == id);

        if (hasTransactions)
            throw new ApplicationException(
                "Não é possível excluir esta categoria pois ela possui transações associadas.");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }
}
