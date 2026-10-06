using FinTrack.Domain.Enums;

namespace FinTrack.Application.RecurringTransactions;

public class RecurringTransactionResponse
{
    public int Id { get; set; }

    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;

    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    public TransactionType Type { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public RecurrenceType Recurrence { get; set; }

    public DateTime NextExecutionDate { get; set; }

    public bool IsActive { get; set; }
}