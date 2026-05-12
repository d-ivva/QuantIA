namespace QuantIA.DTOs;

public class BudgetReportDto
{
    public bool HasBudget { get; set; }
    public decimal? BudgetAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public double PercentageUsed { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public List<CategorySpendingDto> ByCategory { get; set; } = new();
}

public class CategorySpendingDto
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public double Percentage { get; set; }
}
