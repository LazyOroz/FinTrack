using System.Security.Claims;
using FinTrack.Application.RecurringTransactions;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;
using FinTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Api.Controllers;

[ApiController]
[Route("api/recurring-transactions")]
[Authorize]
public class RecurringTransactionsController : ControllerBase
{
    private readonly FinTrackDbContext _context;

    public RecurringTransactionsController(FinTrackDbContext context)
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
    public async Task<ActionResult<IEnumerable<RecurringTransactionResponse>>> GetAll()
    {
        var userId = GetUserId();

        var items = await _context.RecurringTransactions
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.NextExecutionDate)
            .Select(x => new RecurringTransactionResponse
            {
                Id = x.Id,

                AccountId = x.AccountId,
                AccountName = x.Account.Name,

                CategoryId = x.CategoryId,
                CategoryName = x.Category != null
                    ? x.Category.Name
                    : null,

                Type = x.Type,
                Amount = x.Amount,
                Description = x.Description,
                Recurrence = x.Recurrence,
                NextExecutionDate = x.NextExecutionDate,
                IsActive = x.IsActive
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RecurringTransactionResponse>> GetById(int id)
    {
        var userId = GetUserId();

        var item = await _context.RecurringTransactions
            .Where(x =>
                x.Id == id &&
                x.UserId == userId)
            .Select(x => new RecurringTransactionResponse
            {
                Id = x.Id,

                AccountId = x.AccountId,
                AccountName = x.Account.Name,

                CategoryId = x.CategoryId,
                CategoryName = x.Category != null
                    ? x.Category.Name
                    : null,

                Type = x.Type,
                Amount = x.Amount,
                Description = x.Description,
                Recurrence = x.Recurrence,
                NextExecutionDate = x.NextExecutionDate,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();

        if (item is null)
        {
            return NotFound(new
            {
                message = "Recurring transaction not found."
            });
        }

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<RecurringTransactionResponse>> Create(
        CreateRecurringTransactionRequest request)
    {
        var userId = GetUserId();

        if (request.Amount <= 0)
        {
            return BadRequest(new
            {
                message = "Amount must be greater than zero."
            });
        }

        if (request.Type == TransactionType.Transfer)
        {
            return BadRequest(new
            {
                message = "Recurring transfers are not supported yet."
            });
        }

        var account = await _context.Accounts
            .FirstOrDefaultAsync(x =>
                x.Id == request.AccountId &&
                x.UserId == userId);

        if (account is null)
        {
            return BadRequest(new
            {
                message = "Account not found."
            });
        }

        Category? category = null;

        if (request.CategoryId.HasValue)
        {
            category = await _context.Categories
                .FirstOrDefaultAsync(x =>
                    x.Id == request.CategoryId.Value &&
                    x.UserId == userId);

            if (category is null)
            {
                return BadRequest(new
                {
                    message = "Category not found."
                });
            }

            if (category.Type != request.Type)
            {
                return BadRequest(new
                {
                    message = "Category type does not match transaction type."
                });
            }
        }

        var recurring = new RecurringTransaction
        {
            UserId = userId,
            AccountId = account.Id,
            CategoryId = category?.Id,
            Type = request.Type,
            Amount = request.Amount,
            Description = request.Description,
            Recurrence = request.Recurrence,
            NextExecutionDate = request.NextExecutionDate.ToUniversalTime(),
            IsActive = true
        };

        _context.RecurringTransactions.Add(recurring);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = recurring.Id },
            new RecurringTransactionResponse
            {
                Id = recurring.Id,

                AccountId = account.Id,
                AccountName = account.Name,

                CategoryId = category?.Id,
                CategoryName = category?.Name,

                Type = recurring.Type,
                Amount = recurring.Amount,
                Description = recurring.Description,
                Recurrence = recurring.Recurrence,
                NextExecutionDate = recurring.NextExecutionDate,
                IsActive = recurring.IsActive
            });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<RecurringTransactionResponse>> Update(
        int id,
        UpdateRecurringTransactionRequest request)
    {
        var userId = GetUserId();

        if (request.Amount <= 0)
        {
            return BadRequest(new
            {
                message = "Amount must be greater than zero."
            });
        }

        var recurring = await _context.RecurringTransactions
            .Include(x => x.Account)
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.UserId == userId);

        if (recurring is null)
        {
            return NotFound(new
            {
                message = "Recurring transaction not found."
            });
        }

        recurring.Amount = request.Amount;
        recurring.Description = request.Description;
        recurring.Recurrence = request.Recurrence;
        recurring.NextExecutionDate =
            request.NextExecutionDate.ToUniversalTime();
        recurring.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return Ok(new RecurringTransactionResponse
        {
            Id = recurring.Id,

            AccountId = recurring.AccountId,
            AccountName = recurring.Account.Name,

            CategoryId = recurring.CategoryId,
            CategoryName = recurring.Category?.Name,

            Type = recurring.Type,
            Amount = recurring.Amount,
            Description = recurring.Description,
            Recurrence = recurring.Recurrence,
            NextExecutionDate = recurring.NextExecutionDate,
            IsActive = recurring.IsActive
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();

        var recurring = await _context.RecurringTransactions
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.UserId == userId);

        if (recurring is null)
        {
            return NotFound(new
            {
                message = "Recurring transaction not found."
            });
        }

        _context.RecurringTransactions.Remove(recurring);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}