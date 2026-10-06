using FinTrack.Domain.Enums;

namespace FinTrack.Application.Accounts;

public class UpdateAccountRequest
{
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public string Currency { get; set; } = "USD";
}