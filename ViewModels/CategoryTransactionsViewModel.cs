using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Models;
using ExpenseTracker.Views;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.ViewModels;

public partial class CategoryTransactionsViewModel : ObservableObject
{
    [ObservableProperty]
    private int categoryId;

    [ObservableProperty]
    private string categoryName = string.Empty;

    [ObservableProperty]
    private CategoryType categoryType;

    [ObservableProperty]
    private int period; // 0=Weekly, 1=Monthly, 2=Annually, 3=Period

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
    private ObservableCollection<SubCategoryItem> subCategories = [];

    [ObservableProperty]
    private SubCategoryItem? selectedSubCategory;

    [ObservableProperty]
    private ObservableCollection<TransactionItemViewModel> transactions = [];

    public CategoryTransactionsViewModel()
    {
        // Initialize with default values
        InitializeCurrentDates();
    }

    partial void OnSelectedSubCategoryChanged(SubCategoryItem? value)
    {
        // Update IsSelected for all subcategories
        foreach (var sub in SubCategories)
        {
            sub.IsSelected = (sub.Id == value?.Id);
        }
        
        LoadTransactions();
    }

    [RelayCommand]
    private void Select(SubCategoryItem item)
    {
        SelectedSubCategory = item;
    }

    [RelayCommand]
    private void Close()
    {
        if (Application.Current.MainWindow is Window owner)
        {
            var window = owner.OwnedWindows.OfType<Window>().FirstOrDefault(w => w is CategoryTransactionsWindow);
            window?.Close();
        }
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

    public void LoadData()
    {
        LoadSubCategories();
        LoadTransactions();
    }

    private void LoadSubCategories()
    {
        using AppDbContext dbContext = new();

        var category = dbContext.Categories
            .Include(c => c.SubCategories)
            .FirstOrDefault(c => c.Id == CategoryId);

        if (category == null)
            return;

        // Get transactions for this category in the selected period
        var transactions = dbContext.Transactions
            .Include(t => t.SubCategory)
            .Where(t => t.CategoryId == CategoryId)
            .AsQueryable();

        // Filter by selected period
        switch (Period)
        {
            case 0: // Weekly
                transactions = transactions.Where(t => t.Date >= CurrentWeekStart && t.Date <= CurrentWeekEnd);
                break;
            case 1: // Monthly
                transactions = transactions.Where(t => t.Date.Year == CurrentYear && t.Date.Month == CurrentMonth);
                break;
            case 2: // Annually
                transactions = transactions.Where(t => t.Date.Year == CurrentYear);
                break;
            case 3: // Period
                if (PeriodStartDate.HasValue && PeriodEndDate.HasValue)
                {
                    transactions = transactions.Where(t => t.Date >= PeriodStartDate.Value && t.Date <= PeriodEndDate.Value);
                }
                break;
        }

        var transactionList = transactions.ToList();
        decimal totalAmount = transactionList.Sum(t => t.Amount);

        var subCategoryItems = new List<SubCategoryItem>();

        // Add "All" option
        subCategoryItems.Add(new SubCategoryItem
        {
            Id = -1, // Special ID for "All"
            Name = "All",
            Percentage = 100.0,
            Amount = totalAmount,
            Color = Colors.Gray
        });

        // Add subcategories
        var subCategoryGroups = transactionList
            .Where(t => t.SubCategoryId.HasValue)
            .GroupBy(t => new { Id = t.SubCategoryId.Value, Name = t.SubCategory!.Name })
            .Select(g => new
            {
                Id = g.Key.Id,
                Name = g.Key.Name,
                Amount = g.Sum(t => t.Amount)
            })
            .ToList();

        int colorIndex = 0;
        foreach (var sub in subCategoryGroups)
        {
            double percentage = totalAmount > 0 ? (double)(sub.Amount / totalAmount) * 100 : 0;
            subCategoryItems.Add(new SubCategoryItem
            {
                Id = sub.Id,
                Name = sub.Name,
                Percentage = percentage,
                Amount = sub.Amount,
                Color = GetPieColor(colorIndex++)
            });
        }

        SubCategories = new ObservableCollection<SubCategoryItem>(subCategoryItems);
        SelectedSubCategory = SubCategories.FirstOrDefault();
    }

    private void LoadTransactions()
    {
        Transactions.Clear();

        if (SelectedSubCategory == null)
            return;

        using AppDbContext dbContext = new();

        var transactions = dbContext.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Include(t => t.SubCategory)
            .Where(t => t.CategoryId == CategoryId)
            .AsQueryable();

        // Filter by selected period
        switch (Period)
        {
            case 0: // Weekly
                transactions = transactions.Where(t => t.Date >= CurrentWeekStart && t.Date <= CurrentWeekEnd);
                break;
            case 1: // Monthly
                transactions = transactions.Where(t => t.Date.Year == CurrentYear && t.Date.Month == CurrentMonth);
                break;
            case 2: // Annually
                transactions = transactions.Where(t => t.Date.Year == CurrentYear);
                break;
            case 3: // Period
                if (PeriodStartDate.HasValue && PeriodEndDate.HasValue)
                {
                    transactions = transactions.Where(t => t.Date >= PeriodStartDate.Value && t.Date <= PeriodEndDate.Value);
                }
                break;
        }

        // Filter by subcategory (if not "All")
        if (SelectedSubCategory != null && SelectedSubCategory.Id != -1)
        {
            transactions = transactions.Where(t => t.SubCategoryId == SelectedSubCategory.Id);
        }

        var transactionList = transactions
            .OrderByDescending(t => t.Date)
            .ToList();

        foreach (var transaction in transactionList)
        {
            Transactions.Add(new TransactionItemViewModel(transaction, 1, false));
        }
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

public partial class SubCategoryItem : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private double percentage;

    [ObservableProperty]
    private decimal amount;

    [ObservableProperty]
    private Color color;

    [ObservableProperty]
    private bool isSelected;
}
