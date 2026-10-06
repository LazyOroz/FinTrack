using System.Security.Claims;
using FinTrack.Application.Transactions;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;
using FinTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Api.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly FinTrackDbContext _context;

    public TransactionsController(FinTrackDbContext context)
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
    public async Task<ActionResult<IEnumerable<TransactionResponse>>> GetAll()
    {
        var userId = GetUserId();

        var transactions = await _context.Transactions
            .Where(x => x.Account.UserId == userId)
            .OrderByDescending(x => x.TransactionDate)
            .Select(x => new TransactionResponse
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
                TransactionDate = x.TransactionDate
            })
            .ToListAsync();

        return Ok(transactions);
    }

    [HttpPost]
    public async Task<ActionResult<TransactionResponse>> Create(
        CreateTransactionRequest request)
    {
        var userId = GetUserId();

        if (request.Amount <= 0)
            return BadRequest(new
            {
                message = "Amount must be greater than zero."
            });

        if (request.Type == TransactionType.Transfer)
            return BadRequest(new
            {
                message = "Use the transfer endpoint for transfers."
            });

        var account = await _context.Accounts
            .FirstOrDefaultAsync(x =>
                x.Id == request.AccountId &&
                x.UserId == userId);

        if (account is null)
            return NotFound(new
            {
                message = "Account not found."
            });

        Category? category = null;

        if (request.CategoryId.HasValue)
        {
            category = await _context.Categories
                .FirstOrDefaultAsync(x =>
                    x.Id == request.CategoryId.Value &&
                    x.UserId == userId);

            if (category is null)
                return BadRequest(new
                {
                    message = "Category not found."
                });

            if (category.Type != request.Type)
                return BadRequest(new
                {
                    message = "Category type does not match transaction type."
                });
        }

        if (request.Type == TransactionType.Expense &&
            account.Balance < request.Amount)
        {
            return BadRequest(new
            {
                message = "Insufficient balance."
            });
        }

        if (request.Type == TransactionType.Income)
        {
            account.Balance += request.Amount;
        }
        else if (request.Type == TransactionType.Expense)
        {
            account.Balance -= request.Amount;
        }

        var transaction = new Transaction
        {
            AccountId = account.Id,
            CategoryId = category?.Id,
            Type = request.Type,
            Amount = request.Amount,
            Description = request.Description,
            TransactionDate = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);

        await _context.SaveChangesAsync();

        return Ok(new TransactionResponse
        {
            Id = transaction.Id,
            AccountId = account.Id,
            AccountName = account.Name,
            CategoryId = category?.Id,
            CategoryName = category?.Name,
            Type = transaction.Type,
            Amount = transaction.Amount,
            Description = transaction.Description,
            TransactionDate = transaction.TransactionDate
        });
    }
    [HttpPost("transfer")]
    public async Task<ActionResult<TransferResponse>> Transfer(
        TransferRequest request)
    {
        var userId = GetUserId();

        if (request.Amount <= 0)
        {
            return BadRequest(new
            {
                message = "Amount must be greater than zero."
            });
        }

        if (request.FromAccountId == request.ToAccountId)
        {
            return BadRequest(new
            {
                message = "Source and destination accounts must be different."
            });
        }

        var fromAccount = await _context.Accounts
            .FirstOrDefaultAsync(x =>
                x.Id == request.FromAccountId &&
                x.UserId == userId);

        if (fromAccount is null)
        {
            return NotFound(new
            {
                message = "Source account not found."
            });
        }

        var toAccount = await _context.Accounts
            .FirstOrDefaultAsync(x =>
                x.Id == request.ToAccountId &&
                x.UserId == userId);

        if (toAccount is null)
        {
            return NotFound(new
            {
                message = "Destination account not found."
            });
        }

        if (fromAccount.Currency != toAccount.Currency)
        {
            return BadRequest(new
            {
                message = "Accounts must use the same currency."
            });
        }

        if (fromAccount.Balance < request.Amount)
        {
            return BadRequest(new
            {
                message = "Insufficient balance."
            });
        }

        await using var dbTransaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            fromAccount.Balance -= request.Amount;
            toAccount.Balance += request.Amount;

            var transactionDate = DateTime.UtcNow;

            var outgoingTransaction = new Transaction
            {
                AccountId = fromAccount.Id,
                CategoryId = null,
                Type = TransactionType.Transfer,
                Amount = -request.Amount,
                Description = request.Description,
                TransactionDate = transactionDate
            };

            var incomingTransaction = new Transaction
            {
                AccountId = toAccount.Id,
                CategoryId = null,
                Type = TransactionType.Transfer,
                Amount = request.Amount,
                Description = request.Description,
                TransactionDate = transactionDate
            };

            _context.Transactions.AddRange(
                outgoingTransaction,
                incomingTransaction);

            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return Ok(new TransferResponse
            {
                FromAccountId = fromAccount.Id,
                FromAccountName = fromAccount.Name,

                ToAccountId = toAccount.Id,
                ToAccountName = toAccount.Name,

                Amount = request.Amount,

                FromAccountBalance = fromAccount.Balance,
                ToAccountBalance = toAccount.Balance,

                Description = request.Description,
                CreatedAt = transactionDate
            });
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

}