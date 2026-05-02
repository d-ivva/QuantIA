using QuantIA.Models;

namespace QuantIA.Interface;

public interface ITransactionTypeService
{
    Task<TransactionType> Criar(TransactionType request);
    Task<List<TransactionType>> Listar();
    Task<TransactionType?> BuscarPorId(int id);
    Task Atualizar(int id, TransactionType request);
    Task Deletar(int id);
}
