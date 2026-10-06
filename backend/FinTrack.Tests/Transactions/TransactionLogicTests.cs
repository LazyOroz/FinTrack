using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;
using FinTrack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Tests.Transactions;

public class TransactionLogicTests
{
    private static FinTrackDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<FinTrackDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new FinTrackDbContext(options);
    }

    [Fact]
    public async Task Income_ShouldIncreaseAccountBalance()
    {
        await using var context = CreateContext();

        var user = new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = "income@test.com",
            PasswordHash = "hash"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var account = new Account
        {
            UserId = user.Id,
            Name = "Main Card",
            Type = AccountType.BankAccount,
            Balance = 1000,
            Currency = "USD"
        };

        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        const decimal amount = 500;

        account.Balance += amount;

        var transaction = new Transaction
        {
            AccountId = account.Id,
            Type = TransactionType.Income,
            Amount = amount,
            Description = "Salary",
            TransactionDate = DateTime.UtcNow
        };

        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        var savedAccount = await context.Accounts.FindAsync(account.Id);

        Assert.NotNull(savedAccount);
        Assert.Equal(1500m, savedAccount.Balance);

        var savedTransaction =
            await context.Transactions.SingleAsync();

        Assert.Equal(TransactionType.Income, savedTransaction.Type);
        Assert.Equal(500m, savedTransaction.Amount);
    }

    [Fact]
    public async Task Expense_ShouldDecreaseAccountBalance()
    {
        await using var context = CreateContext();

        var user = new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = "expense@test.com",
            PasswordHash = "hash"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var account = new Account
        {
            UserId = user.Id,
            Name = "Main Card",
            Type = AccountType.BankAccount,
            Balance = 1000,
            Currency = "USD"
        };

        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        const decimal amount = 250;

        Assert.True(account.Balance >= amount);

        account.Balance -= amount;

        var transaction = new Transaction
        {
            AccountId = account.Id,
            Type = TransactionType.Expense,
            Amount = amount,
            Description = "Food",
            TransactionDate = DateTime.UtcNow
        };

        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        var savedAccount = await context.Accounts.FindAsync(account.Id);

        Assert.NotNull(savedAccount);
        Assert.Equal(750m, savedAccount.Balance);
    }

    [Fact]
    public async Task Expense_ShouldNotBeApplied_WhenBalanceIsInsufficient()
    {
        await using var context = CreateContext();

        var user = new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = "insufficient@test.com",
            PasswordHash = "hash"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var account = new Account
        {
            UserId = user.Id,
            Name = "Main Card",
            Type = AccountType.BankAccount,
            Balance = 100,
            Currency = "USD"
        };

        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        const decimal amount = 500;

        var canSpend = account.Balance >= amount;

        Assert.False(canSpend);
        Assert.Equal(100m, account.Balance);

        var transactionCount =
            await context.Transactions.CountAsync();

        Assert.Equal(0, transactionCount);
    }

    [Fact]
    public async Task Transfer_ShouldMoveMoneyBetweenAccounts()
    {
        await using var context = CreateContext();

        var user = new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = "transfer@test.com",
            PasswordHash = "hash"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var mainAccount = new Account
        {
            UserId = user.Id,
            Name = "Main Card",
            Type = AccountType.BankAccount,
            Balance = 1000,
            Currency = "USD"
        };

        var savingsAccount = new Account
        {
            UserId = user.Id,
            Name = "Savings",
            Type = AccountType.Savings,
            Balance = 200,
            Currency = "USD"
        };

        context.Accounts.AddRange(
            mainAccount,
            savingsAccount);

        await context.SaveChangesAsync();

        const decimal amount = 300;

        Assert.True(mainAccount.Balance >= amount);
        Assert.Equal(mainAccount.Currency, savingsAccount.Currency);

        mainAccount.Balance -= amount;
        savingsAccount.Balance += amount;

        context.Transactions.AddRange(
            new Transaction
            {
                AccountId = mainAccount.Id,
                Type = TransactionType.Transfer,
                Amount = -amount,
                Description = "Transfer to savings",
                TransactionDate = DateTime.UtcNow
            },
            new Transaction
            {
                AccountId = savingsAccount.Id,
                Type = TransactionType.Transfer,
                Amount = amount,
                Description = "Transfer from Main Card",
                TransactionDate = DateTime.UtcNow
            });

        await context.SaveChangesAsync();

        Assert.Equal(700m, mainAccount.Balance);
        Assert.Equal(500m, savingsAccount.Balance);

        var transactions =
            await context.Transactions.ToListAsync();

        Assert.Equal(2, transactions.Count);
        Assert.Contains(
            transactions,
            x => x.Amount == -300m);

        Assert.Contains(
            transactions,
            x => x.Amount == 300m);
    }
}