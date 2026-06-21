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
    private int selectedPeriod = 1; // 0=Weekly, 1=Monthly (default), 2=Annually, 3=Period

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

    public bool IsWeeklyPeriod => SelectedPeriod == 0;
    public bool IsMonthlyPeriod => SelectedPeriod == 1;
    public bool IsAnnuallyPeriod => SelectedPeriod == 2;
    public bool IsPeriodPeriod => SelectedPeriod == 3;

    public StatsViewModel()
    {
        LoadStatistics();
    }

    partial void OnSelectedTabChanged(int value)
    {
        OnPropertyChanged(nameof(IsIncomeTab));
        OnPropertyChanged(nameof(IsExpenseTab));
    }

    partial void OnSelectedPeriodChanged(int value)
    {
        OnPropertyChanged(nameof(IsWeeklyPeriod));
        OnPropertyChanged(nameof(IsMonthlyPeriod));
        OnPropertyChanged(nameof(IsAnnuallyPeriod));
        OnPropertyChanged(nameof(IsPeriodPeriod));
        LoadStatistics();
    }

    [RelayCommand]
    private void SelectIncomeTab() => SelectedTab = 0;

    [RelayCommand]
    private void SelectExpenseTab() => SelectedTab = 1;

    [RelayCommand]
    private void SelectWeeklyPeriod() => SelectedPeriod = 0;

    [RelayCommand]
    private void SelectMonthlyPeriod() => SelectedPeriod = 1;

    [RelayCommand]
    private void SelectAnnuallyPeriod() => SelectedPeriod = 2;

    [RelayCommand]
    private void SelectPeriodPeriod() => SelectedPeriod = 3;

    private void LoadStatistics()
    {
        using AppDbContext dbContext = new();

        var transactions = dbContext.Transactions
            .Include(t => t.Category)
            .AsQueryable();

        // Filter by selected period
        DateTime now = DateTime.Now;
        switch (SelectedPeriod)
        {
            case 0: // Weekly - last 7 days
                transactions = transactions.Where(t => t.Date >= now.AddDays(-7));
                break;
            case 1: // Monthly - current month
                transactions = transactions.Where(t => t.Date.Year == now.Year && t.Date.Month == now.Month);
                break;
            case 2: // Annually - current year
                transactions = transactions.Where(t => t.Date.Year == now.Year);
                break;
            case 3: // Period - all time (no filter)
                break;
        }

        var transactionList = transactions.ToList();

        // Load income data
        var incomeByCategory = transactionList
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
        var expenseByCategory = transactionList
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
