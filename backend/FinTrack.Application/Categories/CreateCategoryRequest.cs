using FinTrack.Domain.Enums;

namespace FinTrack.Application.Categories;

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
}