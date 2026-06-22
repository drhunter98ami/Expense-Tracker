namespace ExpenseTracker.Models;

public enum TransactionType
{
    Income,
    Expense,
    Transfer
}

public class Transaction
{
    public int Id { get; set; }

    public TransactionType Type { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "SYP";

    public DateTime Date { get; set; }

    public string? Description { get; set; }

    public int? AccountId { get; set; }

    public Account? Account { get; set; }

    public int? CategoryId { get; set; }

    public Category? Category { get; set; }

    public int? SubCategoryId { get; set; }

    public Category? SubCategory { get; set; }

    public int? FromAccountId { get; set; }

    public Account? FromAccount { get; set; }

    public int? ToAccountId { get; set; }

    public Account? ToAccount { get; set; }

    public decimal ExchangeRate { get; set; } = 1;
}
