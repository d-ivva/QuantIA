using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuantIA.Interface;

namespace QuantIA.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReportsController : AuthenticatedControllerBase
{
    private readonly IMonthlyBudgetService _budgetService;

    public ReportsController(IMonthlyBudgetService budgetService, ICurrentUserService currentUserService)
        : base(currentUserService)
    {
        _budgetService = budgetService;
    }

    [HttpGet("dashboard/{month}/{year}")]
    public async Task<IActionResult> GetDashboardData(int month, int year, [FromQuery] int? accountId = null)
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            var data = await _budgetService.GetDashboardData(month, year, userId, accountId);
            return Ok(data);
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
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
            var userId = await GetCurrentUserIdAsync();
            var data = await _budgetService.GetAnnualReport(year, userId);
            return Ok(data);
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
