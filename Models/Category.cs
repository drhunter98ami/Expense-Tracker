namespace ExpenseTracker.Models;

public class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public CategoryType Type { get; set; }

    public int? ParentCategoryId { get; set; }

    public Category? ParentCategory { get; set; }

    public ICollection<Category> SubCategories { get; set; } = [];

    public ICollection<Transaction> Transactions { get; set; } = [];
}
