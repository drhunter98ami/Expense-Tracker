namespace ExpenseTracker.Models;

public class AppSetting
{
    public int Id { get; set; }

    public decimal UsdToSypRate { get; set; }

    public string CurrencyCode { get; set; } = "SYP";

    public bool IsDarkMode { get; set; } = true;

    public bool IsEnglish { get; set; } = false;
}
