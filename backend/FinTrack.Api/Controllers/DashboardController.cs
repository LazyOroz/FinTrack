using System.Security.Claims;
using FinTrack.Application.Dashboard;
using FinTrack.Domain.Enums;
using FinTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly FinTrackDbContext _context;

    public DashboardController(FinTrackDbContext context)
    {
        _context = context;
    }

    private int GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(value, out var userId))
            throw new UnauthorizedAccessException();

        return userId;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryResponse>> GetSummary()
    {
        var userId = GetUserId();

        var totalBalance = await _context.Accounts
            .Where(x => x.UserId == userId)
            .SumAsync(x => x.Balance);

        var totalIncome = await _context.Transactions
            .Where(x =>
                x.Account.UserId == userId &&
                x.Type == TransactionType.Income)
            .SumAsync(x => x.Amount);

        var totalExpenses = await _context.Transactions
            .Where(x =>
                x.Account.UserId == userId &&
                x.Type == TransactionType.Expense)
            .SumAsync(x => x.Amount);

        var accountsCount = await _context.Accounts
            .CountAsync(x => x.UserId == userId);

        var transactionsCount = await _context.Transactions
            .CountAsync(x => x.Account.UserId == userId);

        return Ok(new DashboardSummaryResponse
        {
            TotalBalance = totalBalance,
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            AccountsCount = accountsCount,
            TransactionsCount = transactionsCount
        });
    }
}