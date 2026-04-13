using QuantIA.Models;

namespace QuantIA.Services;

public interface ICategoryService
{
    Task<Category> Criar(Category request);
    Task<List<Category>> Listar();
    Task<Category?> BuscarPorId(int id);
    Task Atualizar(int id, Category request);
    Task Deletar(int id);
}