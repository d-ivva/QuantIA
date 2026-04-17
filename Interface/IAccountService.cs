using QuantIA.Models;

namespace QuantIA.Interface;

public interface IAccountService
{
    Task<Account> Criar(Account request);
    Task<List<Account>> Listar();
    Task<Account?> BuscarPorId(int id);
    Task Atualizar(int id, Account request);
    Task Deletar(int id);
}