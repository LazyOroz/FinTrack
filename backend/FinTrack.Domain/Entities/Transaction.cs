using FinTrack.Domain.Enums;

namespace FinTrack.Domain.Entities;

public class Transaction
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public int? CategoryId { get; set; }

    public TransactionType Type { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Account Account { get; set; } = null!;

    public Category? Category { get; set; }
}