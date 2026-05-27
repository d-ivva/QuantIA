using QuantIA.Models;

namespace QuantIA.Interface;

public interface ITransactionTypeService
{
    Task<TransactionType>        Criar(TransactionType request, int userId);
    Task<List<TransactionType>>  Listar(int userId);
    Task<TransactionType?>       BuscarPorId(int id, int userId);
    Task                         Atualizar(int id, TransactionType request, int userId);
    Task                         Deletar(int id, int userId);
}
