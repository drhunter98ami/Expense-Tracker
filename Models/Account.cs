namespace ExpenseTracker.Models;

public class Account
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Group { get; set; } = "Cash";

    public string Currency { get; set; } = "SYP";

    public ICollection<Transaction> Transactions { get; set; } = [];
}
