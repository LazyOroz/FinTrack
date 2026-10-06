using FinTrack.Domain.Enums;

namespace FinTrack.Application.Accounts;

public class CreateAccountRequest
{
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "USD";
}