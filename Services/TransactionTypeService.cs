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

        public async Task<TransactionType> Criar(TransactionType request)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new Exception("Nome do tipo de transação é obrigatório.");

            if (!Enum.IsDefined(typeof(TransactionDirection), request.Direction))
                throw new Exception("Direção do tipo de transação é inválida.");

            if (string.IsNullOrWhiteSpace(request.Icon))
                throw new Exception("Ícone do tipo de transação é obrigatório.");

            _context.TransactionTypes.Add(request);
            await _context.SaveChangesAsync();

            return request;
        }

        public async Task<List<TransactionType>> Listar()
        {
            using var _context = await _contextFactory.CreateDbContextAsync();

            return await _context.TransactionTypes.ToListAsync();
        }

        public async Task<TransactionType?> BuscarPorId(int id)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();

            return await _context.TransactionTypes
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task Atualizar(int id, TransactionType request)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();

            var transactionType = await _context.TransactionTypes.FindAsync(id)
                ?? throw new Exception("Tipo de transação não encontrado.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new Exception("Nome do tipo de transação é obrigatório.");

            if (!Enum.IsDefined(typeof(TransactionDirection), request.Direction))
                throw new Exception("Direção do tipo de transação é inválida.");

            if (string.IsNullOrWhiteSpace(request.Icon))
                throw new Exception("Ícone do tipo de transação é obrigatório.");

            transactionType.Name = request.Name;
            transactionType.Direction = request.Direction;
            transactionType.Icon = request.Icon;

            await _context.SaveChangesAsync();
        }

        public async Task Deletar(int id)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();

            var transactionType = await _context.TransactionTypes.FindAsync(id)
                ?? throw new Exception("Tipo de transação não encontrado.");
            
            var hasTransactions = await _context.Transactions
                .AnyAsync(t => t.TransactionTypeId == id);

            if (hasTransactions)
                throw new Exception("Não é possível deletar tipo de transação associado a transações.");

            _context.TransactionTypes.Remove(transactionType);
            await _context.SaveChangesAsync();
        }
    }
