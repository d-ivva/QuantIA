using QuantIA.Models;

namespace QuantIA.Interface;

public interface IFinancialProfileService
{
    Task<FinancialProfile> Criar(FinancialProfile request, int userId);
    Task<FinancialProfile?> Buscar(int userId);
    Task Atualizar(FinancialProfile request, int userId);
    Task Deletar(int userId);
}
