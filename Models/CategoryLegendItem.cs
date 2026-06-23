using System.Windows.Media;

namespace ExpenseTracker.Models;

public class CategoryLegendItem
{
    public string CategoryName { get; set; } = string.Empty;
    public Color Color { get; set; }
    public double Percentage { get; set; }
    public decimal TotalAmount { get; set; }
}
