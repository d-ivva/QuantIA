using QuantIA.Models;

namespace QuantIA.Interface;

public interface IAccountService
{
    Task<Account> Criar(Account request, int userId);
    Task<List<Account>> Listar(int userId);
    Task<Account?> BuscarPorId(int id, int userId);
    Task Atualizar(int id, Account request, int userId);
    Task Deletar(int id, int userId);
}
