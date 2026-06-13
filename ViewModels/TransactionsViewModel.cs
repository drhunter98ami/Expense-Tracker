using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Models;
using ExpenseTracker.Services;
using ExpenseTracker.Views;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.ViewModels;

public partial class TransactionsViewModel : ObservableObject
{
    private readonly ObservableCollection<TransactionItemViewModel> _items = [];

    [ObservableProperty]
    private int selectedTab;

    [ObservableProperty]
    private decimal totalIncome;

    [ObservableProperty]
    private decimal totalExpenses;

    [ObservableProperty]
    private decimal netTotal;

    [ObservableProperty]
    private DateTime? selectedCalendarDate = DateTime.Today;

    public bool IsDailyTab => SelectedTab == 0;
    public bool IsCalendarTab => SelectedTab == 1;
    public bool IsMonthlyTab => SelectedTab == 2;
    public bool IsTotalTab => SelectedTab == 3;

    public ICollectionView GroupedTransactions { get; }
    public ICollectionView MonthlyGroupedTransactions { get; }
    public ObservableCollection<TransactionItemViewModel> CalendarTransactions { get; } = [];

    public TransactionsViewModel()
    {
        GroupedTransactions = CollectionViewSource.GetDefaultView(_items);
        GroupedTransactions.GroupDescriptions.Add(
            new PropertyGroupDescription(nameof(TransactionItemViewModel.DateGroup)));

        CollectionViewSource monthlySource = new() { Source = _items };
        MonthlyGroupedTransactions = monthlySource.View;
        MonthlyGroupedTransactions.GroupDescriptions.Add(
            new PropertyGroupDescription(nameof(TransactionItemViewModel.MonthGroup)));

        LoadTransactions();
    }

    partial void OnSelectedTabChanged(int value)
    {
        OnPropertyChanged(nameof(IsDailyTab));
        OnPropertyChanged(nameof(IsCalendarTab));
        OnPropertyChanged(nameof(IsMonthlyTab));
        OnPropertyChanged(nameof(IsTotalTab));
    }

    partial void OnSelectedCalendarDateChanged(DateTime? value) => FilterCalendarTransactions();

    [RelayCommand]
    private void SelectDailyTab() => SelectedTab = 0;

    [RelayCommand]
    private void SelectCalendarTab() => SelectedTab = 1;

    [RelayCommand]
    private void SelectMonthlyTab() => SelectedTab = 2;

    [RelayCommand]
    private void SelectTotalTab() => SelectedTab = 3;

    private void LoadTransactions()
    {
        _items.Clear();

        using AppDbContext dbContext = new();

        List<Transaction> transactions = dbContext.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .OrderByDescending(t => t.Date)
            .ToList();

        decimal income = 0;
        decimal expenses = 0;

        foreach (Transaction transaction in transactions)
        {
            TransactionItemViewModel item = new(transaction);
            _items.Add(item);

            if (item.IsIncome)
                income += transaction.Amount;
            else
                expenses += transaction.Amount;
        }

        TotalIncome = income;
        TotalExpenses = expenses;
        NetTotal = income - expenses;

        FilterCalendarTransactions();
    }

    private void FilterCalendarTransactions()
    {
        CalendarTransactions.Clear();

        DateTime targetDate = SelectedCalendarDate?.Date ?? DateTime.Today;

        foreach (TransactionItemViewModel item in _items)
        {
            if (item.DateGroup == targetDate)
                CalendarTransactions.Add(new TransactionItemViewModel(item.SourceTransaction));
        }
    }

    [RelayCommand]
    private void AddTransaction()
    {
        using AppDbContext dbContext = new();

        if (!dbContext.Accounts.Any() || !dbContext.Categories.Any())
        {
            MessageBox.Show(
                AppUiResources.GetString("MissingLookupMessage"),
                AppUiResources.GetString("MissingLookupTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        AddTransactionWindow dialog = new();

        if (Application.Current.MainWindow is Window owner)
            dialog.Owner = owner;

        if (dialog.ShowDialog() != true)
            return;

        dbContext.Transactions.Add(new Transaction
        {
            Amount = dialog.Amount,
            Date = dialog.TransactionDate,
            Description = dialog.Description,
            AccountId = dialog.AccountId,
            CategoryId = dialog.CategoryId
        });

        dbContext.SaveChanges();
        LoadTransactions();
    }

    public void RefreshLanguage()
    {
        foreach (TransactionItemViewModel item in _items)
            item.RefreshLanguage();

        GroupedTransactions.Refresh();
        MonthlyGroupedTransactions.Refresh();
    }
}

public partial class TransactionItemViewModel : ObservableObject
{
    private readonly Transaction transaction;

    public TransactionItemViewModel(Transaction transaction)
    {
        this.transaction = transaction;
    }

    public Transaction SourceTransaction => transaction;

    public DateTime DateGroup => transaction.Date.Date;

    public string MonthGroup =>
        transaction.Date.ToString("MMMM yyyy", CultureInfo.CurrentUICulture);

    public string Description =>
        string.IsNullOrWhiteSpace(transaction.Description)
            ? AppUiResources.GetString("NoDescriptionText")
            : transaction.Description;

    public decimal Amount => transaction.Amount;

    public bool IsIncome => transaction.Category?.Type == CategoryType.Income;

    public string CategoryName =>
        transaction.Category?.Name ?? AppUiResources.GetString("UncategorizedText");

    public string AccountName =>
        transaction.Account?.Name ?? AppUiResources.GetString("NoAccountText");

    public void RefreshLanguage()
    {
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(CategoryName));
        OnPropertyChanged(nameof(AccountName));
        OnPropertyChanged(nameof(MonthGroup));
    }
}
