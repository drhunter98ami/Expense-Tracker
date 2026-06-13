namespace ExpenseTracker.Models;

public class Account
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Transaction> Transactions { get; set; } = [];
}
