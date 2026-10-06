namespace FinTrack.Application.Budgets;

public class BudgetResponse
{
    public int Id { get; set; }

    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    public decimal LimitAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal PercentageUsed { get; set; }

    public int Month { get; set; }
    public int Year { get; set; }
}