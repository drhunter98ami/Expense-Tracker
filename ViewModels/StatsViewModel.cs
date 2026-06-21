using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Models;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.ViewModels;

public partial class StatsViewModel : ObservableObject
{
    [ObservableProperty]
    private int selectedTab;

    [ObservableProperty]
    private SeriesCollection? incomePieSeries;

    [ObservableProperty]
    private SeriesCollection? expensePieSeries;

    [ObservableProperty]
    private string[]? incomeLabels;

    [ObservableProperty]
    private string[]? expenseLabels;

    [ObservableProperty]
    private Func<ChartPoint, string>? incomeTooltip;

    [ObservableProperty]
    private Func<ChartPoint, string>? expenseTooltip;

    public bool IsIncomeTab => SelectedTab == 0;
    public bool IsExpenseTab => SelectedTab == 1;

    public StatsViewModel()
    {
        LoadStatistics();
    }

    partial void OnSelectedTabChanged(int value)
    {
        OnPropertyChanged(nameof(IsIncomeTab));
        OnPropertyChanged(nameof(IsExpenseTab));
    }

    [RelayCommand]
    private void SelectIncomeTab() => SelectedTab = 0;

    [RelayCommand]
    private void SelectExpenseTab() => SelectedTab = 1;

    private void LoadStatistics()
    {
        using AppDbContext dbContext = new();

        var transactions = dbContext.Transactions
            .Include(t => t.Category)
            .ToList();

        // Load income data
        var incomeByCategory = transactions
            .Where(t => t.Category?.Type == CategoryType.Income)
            .GroupBy(t => t.Category?.Name ?? "Uncategorized")
            .Select(g => new { Category = g.Key, Amount = g.Sum(t => t.Amount) })
            .ToList();

        IncomePieSeries = new SeriesCollection();
        var incomeLabelsList = new List<string>();

        foreach (var item in incomeByCategory)
        {
            IncomePieSeries.Add(new PieSeries
            {
                Title = item.Category,
                Values = new ChartValues<decimal> { item.Amount },
                DataLabels = true
            });
            incomeLabelsList.Add(item.Category);
        }

        IncomeLabels = incomeLabelsList.ToArray();
        IncomeTooltip = chartPoint => 
            string.Format(CultureInfo.CurrentCulture, "{0:N2}%", chartPoint.Participation * 100);

        // Load expense data
        var expenseByCategory = transactions
            .Where(t => t.Category?.Type == CategoryType.Expense)
            .GroupBy(t => t.Category?.Name ?? "Uncategorized")
            .Select(g => new { Category = g.Key, Amount = g.Sum(t => t.Amount) })
            .ToList();

        ExpensePieSeries = new SeriesCollection();
        var expenseLabelsList = new List<string>();

        foreach (var item in expenseByCategory)
        {
            ExpensePieSeries.Add(new PieSeries
            {
                Title = item.Category,
                Values = new ChartValues<decimal> { item.Amount },
                DataLabels = true
            });
            expenseLabelsList.Add(item.Category);
        }

        ExpenseLabels = expenseLabelsList.ToArray();
        ExpenseTooltip = chartPoint => 
            string.Format(CultureInfo.CurrentCulture, "{0:N2}%", chartPoint.Participation * 100);
    }

    public void Refresh()
    {
        LoadStatistics();
    }
}
