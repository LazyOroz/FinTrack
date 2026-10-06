namespace FinTrack.Application.Transactions;

public class TransferResponse
{
    public int FromAccountId { get; set; }
    public string FromAccountName { get; set; } = string.Empty;

    public int ToAccountId { get; set; }
    public string ToAccountName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public decimal FromAccountBalance { get; set; }
    public decimal ToAccountBalance { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
}