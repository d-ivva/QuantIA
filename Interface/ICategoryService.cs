using QuantIA.Models;

namespace QuantIA.Interface;

public interface ICategoryService
{
    Task<Category> Criar(Category request, int userId);
    Task<List<Category>> Listar(int userId);
    Task<Category?> BuscarPorId(int id, int userId);
    Task Atualizar(int id, Category request, int userId);
    Task Deletar(int id, int userId);
}
