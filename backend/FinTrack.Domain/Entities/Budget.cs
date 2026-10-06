namespace FinTrack.Domain.Entities;

public class Budget
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CategoryId { get; set; }

    public decimal LimitAmount { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public User User { get; set; } = null!;

    public Category Category { get; set; } = null!;
}