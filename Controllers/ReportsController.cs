using Microsoft.AspNetCore.Mvc;
using QuantIA.Interface;

namespace QuantIA.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IMonthlyBudgetService _budgetService;

    public ReportsController(IMonthlyBudgetService budgetService)
    {
        _budgetService = budgetService;
    }

    [HttpGet("dashboard/{month}/{year}")]
    public async Task<IActionResult> GetDashboardData(int month, int year)
    {
        try
        {
            var data = await _budgetService.GetDashboardData(month, year);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("annual/{year}")]
    public async Task<IActionResult> GetAnnualReport(int year)
    {
        try
        {
            var data = await _budgetService.GetAnnualReport(year);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}