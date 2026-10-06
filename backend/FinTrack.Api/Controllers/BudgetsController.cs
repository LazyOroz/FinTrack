using System.Security.Claims;
using FinTrack.Application.Budgets;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;
using FinTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Api.Controllers;

[ApiController]
[Route("api/budgets")]
[Authorize]
public class BudgetsController : ControllerBase
{
    private readonly FinTrackDbContext _context;

    public BudgetsController(FinTrackDbContext context)
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
    public async Task<ActionResult<IEnumerable<BudgetResponse>>> GetAll()
    {
        var userId = GetUserId();

        var budgets = await _context.Budgets
            .Where(x => x.UserId == userId)
            .Include(x => x.Category)
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ToListAsync();

        var result = new List<BudgetResponse>();

        foreach (var budget in budgets)
        {
            result.Add(await BuildResponse(budget));
        }

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BudgetResponse>> GetById(int id)
    {
        var userId = GetUserId();

        var budget = await _context.Budgets
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.UserId == userId);

        if (budget is null)
        {
            return NotFound(new
            {
                message = "Budget not found."
            });
        }

        return Ok(await BuildResponse(budget));
    }

    [HttpPost]
    public async Task<ActionResult<BudgetResponse>> Create(
        CreateBudgetRequest request)
    {
        var userId = GetUserId();

        if (request.LimitAmount <= 0)
        {
            return BadRequest(new
            {
                message = "Budget limit must be greater than zero."
            });
        }

        if (request.Month < 1 || request.Month > 12)
        {
            return BadRequest(new
            {
                message = "Month must be between 1 and 12."
            });
        }

        if (request.Year < 2000 || request.Year > 2100)
        {
            return BadRequest(new
            {
                message = "Invalid year."
            });
        }

        var category = await _context.Categories
            .FirstOrDefaultAsync(x =>
                x.Id == request.CategoryId &&
                x.UserId == userId);

        if (category is null)
        {
            return BadRequest(new
            {
                message = "Category not found."
            });
        }

        if (category.Type != TransactionType.Expense)
        {
            return BadRequest(new
            {
                message = "Budgets can only be created for expense categories."
            });
        }

        var alreadyExists = await _context.Budgets
            .AnyAsync(x =>
                x.UserId == userId &&
                x.CategoryId == request.CategoryId &&
                x.Month == request.Month &&
                x.Year == request.Year);

        if (alreadyExists)
        {
            return Conflict(new
            {
                message = "A budget already exists for this category and month."
            });
        }

        var budget = new Budget
        {
            UserId = userId,
            CategoryId = category.Id,
            LimitAmount = request.LimitAmount,
            Month = request.Month,
            Year = request.Year
        };

        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();

        budget.Category = category;

        var response = await BuildResponse(budget);

        return CreatedAtAction(
            nameof(GetById),
            new { id = budget.Id },
            response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BudgetResponse>> Update(
        int id,
        UpdateBudgetRequest request)
    {
        var userId = GetUserId();

        if (request.LimitAmount <= 0)
        {
            return BadRequest(new
            {
                message = "Budget limit must be greater than zero."
            });
        }

        var budget = await _context.Budgets
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.UserId == userId);

        if (budget is null)
        {
            return NotFound(new
            {
                message = "Budget not found."
            });
        }

        budget.LimitAmount = request.LimitAmount;

        await _context.SaveChangesAsync();

        return Ok(await BuildResponse(budget));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();

        var budget = await _context.Budgets
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.UserId == userId);

        if (budget is null)
        {
            return NotFound(new
            {
                message = "Budget not found."
            });
        }

        _context.Budgets.Remove(budget);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<BudgetResponse> BuildResponse(Budget budget)
    {
        var startDate = new DateTime(
            budget.Year,
            budget.Month,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);

        var endDate = startDate.AddMonths(1);

        var spentAmount = await _context.Transactions
            .Where(x =>
                x.CategoryId == budget.CategoryId &&
                x.Account.UserId == budget.UserId &&
                x.Type == TransactionType.Expense &&
                x.TransactionDate >= startDate &&
                x.TransactionDate < endDate)
            .SumAsync(x => x.Amount);

        var remainingAmount =
            budget.LimitAmount - spentAmount;

        var percentageUsed = budget.LimitAmount > 0
            ? Math.Round(
                spentAmount / budget.LimitAmount * 100,
                2)
            : 0;

        return new BudgetResponse
        {
            Id = budget.Id,

            CategoryId = budget.CategoryId,
            CategoryName = budget.Category.Name,

            LimitAmount = budget.LimitAmount,
            SpentAmount = spentAmount,
            RemainingAmount = remainingAmount,
            PercentageUsed = percentageUsed,

            Month = budget.Month,
            Year = budget.Year
        };
    }
}