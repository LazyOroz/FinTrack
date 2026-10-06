namespace FinTrack.Application.Dashboard;

public class DashboardSummaryResponse
{
    public decimal TotalBalance { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public int AccountsCount { get; set; }
    public int TransactionsCount { get; set; }
}