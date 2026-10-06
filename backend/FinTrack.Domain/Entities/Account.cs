using FinTrack.Domain.Enums;

namespace FinTrack.Domain.Entities;

public class Account
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public AccountType Type { get; set; }

    public decimal Balance { get; set; }

    public string Currency { get; set; } = "USD";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;

    public ICollection<Transaction> Transactions { get; set; }
        = new List<Transaction>();
}