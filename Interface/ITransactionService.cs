using QuantIA.Models;

namespace QuantIA.Interface;

public interface ITransactionService
{
    Task<Transaction> Criar(Transaction request, int userId);
    Task<List<Transaction>> Listar(int userId);
    Task<Transaction?> BuscarPorId(int id, int userId);
    Task Atualizar(int id, Transaction request, int userId);
    Task Deletar(int id, int userId);
}
