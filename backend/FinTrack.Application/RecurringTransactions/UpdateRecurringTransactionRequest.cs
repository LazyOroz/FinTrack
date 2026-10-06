using FinTrack.Domain.Enums;

namespace FinTrack.Application.RecurringTransactions;

public class UpdateRecurringTransactionRequest
{
    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public RecurrenceType Recurrence { get; set; }

    public DateTime NextExecutionDate { get; set; }

    public bool IsActive { get; set; }
}