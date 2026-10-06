using System.Security.Claims;
using FinTrack.Application.Accounts;
using FinTrack.Domain.Entities;
using FinTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Api.Controllers;

[ApiController]
[Route("api/accounts")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly FinTrackDbContext _context;

    public AccountsController(FinTrackDbContext context)
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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccountResponse>>> GetAll()
    {
        var userId = GetUserId();

        var accounts = await _context.Accounts
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Name)
            .Select(x => new AccountResponse
            {
                Id = x.Id,
                Name = x.Name,
                Type = x.Type,
                Balance = x.Balance,
                Currency = x.Currency,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return Ok(accounts);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AccountResponse>> GetById(int id)
    {
        var userId = GetUserId();

        var account = await _context.Accounts
            .Where(x => x.Id == id && x.UserId == userId)
            .Select(x => new AccountResponse
            {
                Id = x.Id,
                Name = x.Name,
                Type = x.Type,
                Balance = x.Balance,
                Currency = x.Currency,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (account is null)
            return NotFound();

        return Ok(account);
    }

    [HttpPost]
    public async Task<ActionResult<AccountResponse>> Create(
        CreateAccountRequest request)
    {
        var userId = GetUserId();

        var account = new Account
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Type = request.Type,
            Balance = request.Balance,
            Currency = request.Currency.Trim().ToUpperInvariant()
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var response = new AccountResponse
        {
            Id = account.Id,
            Name = account.Name,
            Type = account.Type,
            Balance = account.Balance,
            Currency = account.Currency,
            CreatedAt = account.CreatedAt
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = account.Id },
            response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateAccountRequest request)
    {
        var userId = GetUserId();

        var account = await _context.Accounts
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.UserId == userId);

        if (account is null)
            return NotFound();

        account.Name = request.Name.Trim();
        account.Type = request.Type;
        account.Currency = request.Currency
            .Trim()
            .ToUpperInvariant();

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();

        var account = await _context.Accounts
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.UserId == userId);

        if (account is null)
            return NotFound();

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}