using FinTrack.Domain.Enums;

namespace FinTrack.Application.Transactions;

public class TransactionResponse
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;

    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }

    public DateTime TransactionDate { get; set; }
}