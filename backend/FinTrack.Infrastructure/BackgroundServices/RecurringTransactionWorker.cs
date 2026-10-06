using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;
using FinTrack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinTrack.Infrastructure.BackgroundServices;

public class RecurringTransactionWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RecurringTransactionWorker> _logger;

    public RecurringTransactionWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<RecurringTransactionWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Recurring transaction worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRecurringTransactions(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing recurring transactions.");
            }

            await Task.Delay(
                TimeSpan.FromMinutes(1),
                stoppingToken);
        }
    }

    private async Task ProcessRecurringTransactions(
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<FinTrackDbContext>();

        var now = DateTime.UtcNow;

        var recurringTransactions =
            await context.RecurringTransactions
                .Include(x => x.Account)
                .Where(x =>
                    x.IsActive &&
                    x.NextExecutionDate <= now)
                .ToListAsync(cancellationToken);

        foreach (var recurring in recurringTransactions)
        {
            if (recurring.Type == TransactionType.Income)
            {
                recurring.Account.Balance += recurring.Amount;
            }
            else if (recurring.Type == TransactionType.Expense)
            {
                if (recurring.Account.Balance < recurring.Amount)
                {
                    _logger.LogWarning(
                        "Recurring transaction {Id} skipped because of insufficient balance.",
                        recurring.Id);

                    recurring.NextExecutionDate =
                        CalculateNextExecutionDate(
                            recurring.NextExecutionDate,
                            recurring.Recurrence);

                    continue;
                }

                recurring.Account.Balance -= recurring.Amount;
            }
            else
            {
                continue;
            }

            var transaction = new Transaction
            {
                AccountId = recurring.AccountId,
                CategoryId = recurring.CategoryId,
                Type = recurring.Type,
                Amount = recurring.Amount,
                Description = recurring.Description,
                TransactionDate = now
            };

            context.Transactions.Add(transaction);

            recurring.NextExecutionDate =
                CalculateNextExecutionDate(
                    recurring.NextExecutionDate,
                    recurring.Recurrence);

            _logger.LogInformation(
                "Recurring transaction {Id} executed.",
                recurring.Id);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static DateTime CalculateNextExecutionDate(
        DateTime currentDate,
        RecurrenceType recurrence)
    {
        return recurrence switch
        {
            RecurrenceType.Daily =>
                currentDate.AddDays(1),

            RecurrenceType.Weekly =>
                currentDate.AddDays(7),

            RecurrenceType.Monthly =>
                currentDate.AddMonths(1),

            RecurrenceType.Yearly =>
                currentDate.AddYears(1),

            _ => throw new ArgumentOutOfRangeException(
                nameof(recurrence),
                recurrence,
                null)
        };
    }
}