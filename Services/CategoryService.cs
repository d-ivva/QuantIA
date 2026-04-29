using Microsoft.EntityFrameworkCore;
using QuantIA.Data;
using QuantIA.Models;

namespace QuantIA.Services;

public class CategoryService : ICategoryService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public CategoryService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Category> Criar(Category request)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ApplicationException("O nome da categoria é obrigatório.");

        var name = request.Name.Trim();

        var exists = await _context.Categories
            .AnyAsync(c => c.Name.ToLower() == name.ToLower());

        if (exists)
            throw new ApplicationException("Já existe uma categoria com esse nome.");

        request.Name = name;
        request.Color = request.Color;

        _context.Categories.Add(request);
        await _context.SaveChangesAsync();

        return request;
    }

    public async Task<List<Category>> Listar()
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        return await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> BuscarPorId(int id)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task Atualizar(int id, Category request)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        var category = await _context.Categories.FindAsync(id)
            ?? throw new ApplicationException("Categoria não encontrada.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ApplicationException("O nome da categoria é obrigatório.");

        var name = request.Name.Trim();

        var exists = await _context.Categories
            .AnyAsync(c => c.Id != id && c.Name.ToLower() == name.ToLower());

        if (exists)
            throw new ApplicationException("Já existe outra categoria com esse nome.");

        category.Name = name;
        category.Color = string.IsNullOrWhiteSpace(request.Color)
            ? "#6B7280"
            : request.Color;

        await _context.SaveChangesAsync();
    }

    public async Task Deletar(int id)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        var category = await _context.Categories.FindAsync(id)
            ?? throw new ApplicationException("Categoria não encontrada.");

        var hasTransactions = await _context.Transactions
            .AnyAsync(t => t.CategoryId == id);

        if (hasTransactions)
            throw new ApplicationException(
                "Não é possível excluir esta categoria pois ela possui transações associadas."
            );

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }
}