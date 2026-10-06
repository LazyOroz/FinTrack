using FinTrack.Domain.Enums;

namespace FinTrack.Application.Transactions;

public class CreateTransactionRequest
{
    public int AccountId { get; set; }
    public int? CategoryId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}