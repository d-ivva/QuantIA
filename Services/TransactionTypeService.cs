using Microsoft.EntityFrameworkCore;
using QuantIA.Data;
using QuantIA.Interface;
using QuantIA.Models;

namespace QuantIA.Services;

public class TransactionTypeService : ITransactionTypeService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public TransactionTypeService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    // Retorna APENAS os tipos do usuário — sem seed, sem fallback para null
    public async Task<List<TransactionType>> Listar(int userId)
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();
        return await ctx.TransactionTypes
            .Where(t => t.UserId == userId)
            .ToListAsync();
    }

    public async Task<TransactionType?> BuscarPorId(int id, int userId)
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();
        return await ctx.TransactionTypes
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    }

    public async Task<TransactionType> Criar(TransactionType request, int userId)
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new Exception("Nome do tipo de transação é obrigatório.");
        if (!Enum.IsDefined(typeof(TransactionDirection), request.Direction))
            throw new Exception("Direção do tipo de transação é inválida.");
        if (string.IsNullOrWhiteSpace(request.Icon))
            throw new Exception("Ícone do tipo de transação é obrigatório.");

        request.UserId = userId;
        ctx.TransactionTypes.Add(request);
        await ctx.SaveChangesAsync();
        return request;
    }

    public async Task Atualizar(int id, TransactionType request, int userId)
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();

        var tt = await ctx.TransactionTypes
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId)
            ?? throw new Exception("Tipo de transação não encontrado.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new Exception("Nome do tipo de transação é obrigatório.");
        if (!Enum.IsDefined(typeof(TransactionDirection), request.Direction))
            throw new Exception("Direção do tipo de transação é inválida.");
        if (string.IsNullOrWhiteSpace(request.Icon))
            throw new Exception("Ícone do tipo de transação é obrigatório.");

        tt.Name      = request.Name;
        tt.Direction = request.Direction;
        tt.Icon      = request.Icon;
        await ctx.SaveChangesAsync();
    }

    public async Task Deletar(int id, int userId)
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();

        var tt = await ctx.TransactionTypes
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId)
            ?? throw new Exception("Tipo de transação não encontrado.");

        var hasTransactions = await ctx.Transactions.AnyAsync(t => t.TransactionTypeId == id);
        if (hasTransactions)
            throw new Exception("Não é possível excluir tipo associado a transações.");

        ctx.TransactionTypes.Remove(tt);
        await ctx.SaveChangesAsync();
    }
}
