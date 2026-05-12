using QuantIA.DTOs;
using QuantIA.Models;

namespace QuantIA.Interface;

public interface IMonthlyBudgetService
{
    Task<MonthlyBudget> Criar(MonthlyBudget request);
    Task<List<MonthlyBudget>> Listar();
    Task<MonthlyBudget?> BuscarPorId(int id);
    Task<MonthlyBudget?> BuscarPorMesAno(int month, int year);
    Task Atualizar(int id, MonthlyBudget request);
    Task Deletar(int id);
    Task<BudgetReportDto> GerarRelatorio(int month, int year);
}
