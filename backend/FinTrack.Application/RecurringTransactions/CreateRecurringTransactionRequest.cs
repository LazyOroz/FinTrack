using FinTrack.Domain.Enums;

namespace FinTrack.Application.RecurringTransactions;

public class CreateRecurringTransactionRequest
{
    public int AccountId { get; set; }
    public int? CategoryId { get; set; }

    public TransactionType Type { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public RecurrenceType Recurrence { get; set; }

    public DateTime NextExecutionDate { get; set; }
}