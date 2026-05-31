using QuantIA.DTOs;
using QuantIA.Models;

namespace QuantIA.Interface;

public interface IMonthlyBudgetService
{
    Task<MonthlyBudget> Criar(MonthlyBudget request, int userId);
    Task<List<MonthlyBudget>> Listar(int userId);
    Task<MonthlyBudget?> BuscarPorId(int id, int userId);
    Task<MonthlyBudget?> BuscarPorMesAno(int month, int year, int userId);
    Task Atualizar(int id, MonthlyBudget request, int userId);
    Task Deletar(int id, int userId);
    Task<BudgetReportDto> GerarRelatorio(int month, int year, int userId);

    
    Task<object> GetDashboardData(int month, int year, int userId, int? accountId = null);
    Task<object> GetAnnualReport(int year, int userId);
}