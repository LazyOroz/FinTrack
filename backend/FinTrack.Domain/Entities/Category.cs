using FinTrack.Domain.Enums;

namespace FinTrack.Domain.Entities;

public class Category
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public TransactionType Type { get; set; }

    public User User { get; set; } = null!;

    public ICollection<Transaction> Transactions { get; set; }
        = new List<Transaction>();
}