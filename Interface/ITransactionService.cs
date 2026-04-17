using QuantIA.Models;

namespace QuantIA.Interface;

public interface ITransactionService
{
    Task<Transaction> Criar(Transaction request);
    Task<List<Transaction>> Listar();
    Task<Transaction?> BuscarPorId(int id);
    Task Atualizar(int id, Transaction request);
    Task Deletar(int id);
}