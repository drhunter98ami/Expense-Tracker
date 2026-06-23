using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Models;
using ExpenseTracker.Views;
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
    private DateTime currentWeekStart;

    [ObservableProperty]
    private DateTime currentWeekEnd;

    [ObservableProperty]
    private int currentMonth;

    [ObservableProperty]
    private int currentYear;

    [ObservableProperty]
    private DateTime? periodStartDate;

    [ObservableProperty]
    private DateTime? periodEndDate;

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

    [ObservableProperty]
    private ObservableCollection<CategoryLegendItem>? incomeLegendItems;

    [ObservableProperty]
    private ObservableCollection<CategoryLegendItem>? expenseLegendItems;

    public bool IsIncomeTab => SelectedTab == 0;
    public bool IsExpenseTab => SelectedTab == 1;

    public SeriesCollection? CurrentPieSeries => IsIncomeTab ? IncomePieSeries : ExpensePieSeries;

    public ObservableCollection<CategoryLegendItem>? CurrentLegendItems => IsIncomeTab ? IncomeLegendItems : ExpenseLegendItems;

    public bool IsWeeklyPeriod => SelectedPeriod == 0;
    public bool IsMonthlyPeriod => SelectedPeriod == 1;
    public bool IsAnnuallyPeriod => SelectedPeriod == 2;
    public bool IsPeriodPeriod => SelectedPeriod == 3;

    public string WeekRangeText => $"{CurrentWeekStart:dd/MM/yyyy} - {CurrentWeekEnd:dd/MM/yyyy}";
    public string MonthYearText => new DateTime(CurrentYear, CurrentMonth, 1).ToString("MMMM yyyy", CultureInfo.CurrentCulture);
    public string YearText => CurrentYear.ToString();

    public StatsViewModel()
    {
        InitializeCurrentDates();
        LoadStatistics();
    }

    private void InitializeCurrentDates()
    {
        DateTime now = DateTime.Now;
        
        // Initialize week (Saturday to Saturday)
        int daysSinceSaturday = ((int)now.DayOfWeek - (int)DayOfWeek.Saturday + 7) % 7;
        CurrentWeekStart = now.AddDays(-daysSinceSaturday).Date;
        CurrentWeekEnd = CurrentWeekStart.AddDays(6).Date;
        
        // Initialize month and year
        CurrentMonth = now.Month;
        CurrentYear = now.Year;
        
        // Initialize period dates (current month by default)
        PeriodStartDate = new DateTime(now.Year, now.Month, 1);
        PeriodEndDate = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
    }

    partial void OnSelectedTabChanged(int value)
    {
        OnPropertyChanged(nameof(IsIncomeTab));
        OnPropertyChanged(nameof(IsExpenseTab));
        OnPropertyChanged(nameof(CurrentPieSeries));
        OnPropertyChanged(nameof(CurrentLegendItems));
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

    [RelayCommand]
    private void ShowCategoryDetails(CategoryLegendItem item)
    {
        if (item == null || !item.CategoryId.HasValue)
            return;

        var window = new CategoryTransactionsWindow
        {
            CategoryId = item.CategoryId.Value,
            CategoryName = item.CategoryName,
            CategoryType = IsIncomeTab ? CategoryType.Income : CategoryType.Expense,
            Period = SelectedPeriod,
            CurrentWeekStart = CurrentWeekStart,
            CurrentWeekEnd = CurrentWeekEnd,
            CurrentMonth = CurrentMonth,
            CurrentYear = CurrentYear,
            PeriodStartDate = PeriodStartDate,
            PeriodEndDate = PeriodEndDate
        };

        if (Application.Current.MainWindow is Window owner)
            window.Owner = owner;

        window.ShowDialog();
    }

    [RelayCommand]
    private void PreviousWeek()
    {
        CurrentWeekStart = CurrentWeekStart.AddDays(-7);
        CurrentWeekEnd = CurrentWeekEnd.AddDays(-7);
        OnPropertyChanged(nameof(WeekRangeText));
        LoadStatistics();
    }

    [RelayCommand]
    private void NextWeek()
    {
        CurrentWeekStart = CurrentWeekStart.AddDays(7);
        CurrentWeekEnd = CurrentWeekEnd.AddDays(7);
        OnPropertyChanged(nameof(WeekRangeText));
        LoadStatistics();
    }

    [RelayCommand]
    private void GoToCurrentWeek()
    {
        DateTime now = DateTime.Now;
        int daysSinceSaturday = ((int)now.DayOfWeek - (int)DayOfWeek.Saturday + 7) % 7;
        CurrentWeekStart = now.AddDays(-daysSinceSaturday).Date;
        CurrentWeekEnd = CurrentWeekStart.AddDays(6).Date;
        OnPropertyChanged(nameof(WeekRangeText));
        LoadStatistics();
    }

    [RelayCommand]
    private void PreviousMonth()
    {
        CurrentMonth--;
        if (CurrentMonth < 1)
        {
            CurrentMonth = 12;
            CurrentYear--;
        }
        OnPropertyChanged(nameof(MonthYearText));
        LoadStatistics();
    }

    [RelayCommand]
    private void NextMonth()
    {
        CurrentMonth++;
        if (CurrentMonth > 12)
        {
            CurrentMonth = 1;
            CurrentYear++;
        }
        OnPropertyChanged(nameof(MonthYearText));
        LoadStatistics();
    }

    [RelayCommand]
    private void GoToCurrentMonth()
    {
        DateTime now = DateTime.Now;
        CurrentMonth = now.Month;
        CurrentYear = now.Year;
        OnPropertyChanged(nameof(MonthYearText));
        LoadStatistics();
    }

    [RelayCommand]
    private void PreviousYear()
    {
        CurrentYear--;
        OnPropertyChanged(nameof(YearText));
        LoadStatistics();
    }

    [RelayCommand]
    private void NextYear()
    {
        CurrentYear++;
        OnPropertyChanged(nameof(YearText));
        LoadStatistics();
    }

    [RelayCommand]
    private void GoToCurrentYear()
    {
        CurrentYear = DateTime.Now.Year;
        OnPropertyChanged(nameof(YearText));
        LoadStatistics();
    }

    partial void OnPeriodStartDateChanged(DateTime? value)
    {
        if (value.HasValue && PeriodEndDate.HasValue && value > PeriodEndDate)
        {
            PeriodEndDate = value;
        }
        LoadStatistics();
    }

    partial void OnPeriodEndDateChanged(DateTime? value)
    {
        if (value.HasValue && PeriodStartDate.HasValue && value < PeriodStartDate)
        {
            PeriodStartDate = value;
        }
        LoadStatistics();
    }

    private void LoadStatistics()
    {
        using AppDbContext dbContext = new();

        var transactions = dbContext.Transactions
            .Include(t => t.Category)
            .AsQueryable();

        // Filter by selected period
        switch (SelectedPeriod)
        {
            case 0: // Weekly - selected week
                transactions = transactions.Where(t => t.Date >= CurrentWeekStart && t.Date <= CurrentWeekEnd);
                break;
            case 1: // Monthly - selected month
                transactions = transactions.Where(t => t.Date.Year == CurrentYear && t.Date.Month == CurrentMonth);
                break;
            case 2: // Annually - selected year
                transactions = transactions.Where(t => t.Date.Year == CurrentYear);
                break;
            case 3: // Period - custom date range
                if (PeriodStartDate.HasValue && PeriodEndDate.HasValue)
                {
                    transactions = transactions.Where(t => t.Date >= PeriodStartDate.Value && t.Date <= PeriodEndDate.Value);
                }
                break;
        }

        var transactionList = transactions.ToList();

        // Load income data
        var incomeByCategory = transactionList
            .Where(t => t.Category?.Type == CategoryType.Income)
            .GroupBy(t => new { Name = t.Category?.Name ?? "Uncategorized", CategoryId = t.Category?.Id })
            .Select(g => new { Category = g.Key.Name, CategoryId = g.Key.CategoryId, Amount = g.Sum(t => t.Currency == "USD" ? t.Amount * t.ExchangeRate : t.Amount) })
            .ToList();

        IncomePieSeries = new SeriesCollection();
        var incomeLabelsList = new List<string>();
        var incomeLegendItemsList = new List<CategoryLegendItem>();
        
        decimal incomeTotal = incomeByCategory.Sum(x => x.Amount);

        foreach (var item in incomeByCategory)
        {
            var color = GetPieColor(incomeByCategory.IndexOf(item));
            IncomePieSeries.Add(new PieSeries
            {
                Title = item.Category,
                Values = new ChartValues<decimal> { item.Amount },
                DataLabels = true,
                Fill = new SolidColorBrush(color)
            });
            incomeLabelsList.Add(item.Category);
            
            double percentage = incomeTotal > 0 ? (double)(item.Amount / incomeTotal) * 100 : 0;
            incomeLegendItemsList.Add(new CategoryLegendItem
            {
                CategoryName = item.Category,
                Color = color,
                Percentage = percentage,
                TotalAmount = item.Amount,
                CategoryId = item.CategoryId
            });
        }

        IncomeLabels = incomeLabelsList.ToArray();
        IncomeTooltip = chartPoint => 
            string.Format(CultureInfo.CurrentCulture, "{0:N2}%", chartPoint.Participation * 100);
        
        IncomeLegendItems = new ObservableCollection<CategoryLegendItem>(
            incomeLegendItemsList.OrderByDescending(x => x.Percentage));

        // Load expense data
        var expenseByCategory = transactionList
            .Where(t => t.Category?.Type == CategoryType.Expense)
            .GroupBy(t => new { Name = t.Category?.Name ?? "Uncategorized", CategoryId = t.Category?.Id })
            .Select(g => new { Category = g.Key.Name, CategoryId = g.Key.CategoryId, Amount = g.Sum(t => t.Currency == "USD" ? t.Amount * t.ExchangeRate : t.Amount) })
            .ToList();

        ExpensePieSeries = new SeriesCollection();
        var expenseLabelsList = new List<string>();
        var expenseLegendItemsList = new List<CategoryLegendItem>();
        
        decimal expenseTotal = expenseByCategory.Sum(x => x.Amount);

        foreach (var item in expenseByCategory)
        {
            var color = GetPieColor(expenseByCategory.IndexOf(item));
            ExpensePieSeries.Add(new PieSeries
            {
                Title = item.Category,
                Values = new ChartValues<decimal> { item.Amount },
                DataLabels = true,
                Fill = new SolidColorBrush(color)
            });
            expenseLabelsList.Add(item.Category);
            
            double percentage = expenseTotal > 0 ? (double)(item.Amount / expenseTotal) * 100 : 0;
            expenseLegendItemsList.Add(new CategoryLegendItem
            {
                CategoryName = item.Category,
                Color = color,
                Percentage = percentage,
                TotalAmount = item.Amount,
                CategoryId = item.CategoryId
            });
        }

        ExpenseLabels = expenseLabelsList.ToArray();
        ExpenseTooltip = chartPoint => 
            string.Format(CultureInfo.CurrentCulture, "{0:N2}%", chartPoint.Participation * 100);
        
        ExpenseLegendItems = new ObservableCollection<CategoryLegendItem>(
            expenseLegendItemsList.OrderByDescending(x => x.Percentage));
    }

    public void Refresh()
    {
        LoadStatistics();
    }

    private Color GetPieColor(int index)
    {
        Color[] colors = new[]
        {
            Color.FromRgb(59, 130, 246),   // Blue
            Color.FromRgb(239, 68, 68),    // Red
            Color.FromRgb(16, 185, 129),   // Green
            Color.FromRgb(245, 158, 11),   // Yellow/Orange
            Color.FromRgb(139, 92, 246),   // Purple
            Color.FromRgb(236, 72, 153),   // Pink
            Color.FromRgb(6, 182, 212),    // Cyan
            Color.FromRgb(249, 115, 22),   // Orange
            Color.FromRgb(99, 102, 241),   // Indigo
            Color.FromRgb(34, 197, 94),    // Emerald
            Color.FromRgb(168, 85, 247),   // Violet
            Color.FromRgb(20, 184, 166),   // Teal
        };
        
        return colors[index % colors.Length];
    }
}
