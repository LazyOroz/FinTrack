namespace FinTrack.Application.Budgets;

public class CreateBudgetRequest
{
    public int CategoryId { get; set; }
    public decimal LimitAmount { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
}