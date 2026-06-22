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

using ExpenseTracker.Services;



namespace ExpenseTracker.ViewModels;



public partial class TransactionsViewModel : ObservableObject

{

    private readonly ObservableCollection<TransactionItemViewModel> _items = [];

    private decimal _usdRate = 15000;

    private bool _globalIsUsd;



    [ObservableProperty]

    private int selectedTab;



    [ObservableProperty]

    private decimal totalIncome;



    [ObservableProperty]

    private decimal totalExpenses;



    [ObservableProperty]

    private decimal netTotal;



    [ObservableProperty]

    private DateTime? selectedCalendarDate = TimeService.Today;



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

        AppSetting settings = AppSettingsService.GetOrCreate();

        _usdRate = settings.UsdToSypRate > 0 ? settings.UsdToSypRate : 1;

        _globalIsUsd = settings.CurrencyCode == "USD";



        _items.Clear();



        using AppDbContext dbContext = new();



        List<Transaction> transactions = dbContext.Transactions

            .Include(t => t.Account)

            .Include(t => t.Category)

            .Include(t => t.SubCategory)

            .Include(t => t.FromAccount)

            .Include(t => t.ToAccount)

            .OrderByDescending(t => t.Date)

            .ToList();



        decimal income = 0;

        decimal expenses = 0;



        foreach (Transaction transaction in transactions)

        {

            TransactionItemViewModel item = new(transaction, _usdRate, _globalIsUsd);

            _items.Add(item);



            if (transaction.Type == TransactionType.Transfer)
            {
                // Transfers don't affect income/expense totals
                continue;
            }

            // Convert amount to global currency for totals calculation
            decimal amountInGlobalCurrency = transaction.Amount;
            decimal storedRate = transaction.ExchangeRate > 0 ? transaction.ExchangeRate : 1;

            if (transaction.Currency == "USD")
            {
                amountInGlobalCurrency = _globalIsUsd ? transaction.Amount : transaction.Amount * storedRate;
            }
            else
            {
                amountInGlobalCurrency = _globalIsUsd ? transaction.Amount / storedRate : transaction.Amount;
            }

            if (item.IsIncome)

                income += amountInGlobalCurrency;

            else

                expenses += amountInGlobalCurrency;

        }



        TotalIncome = income;

        TotalExpenses = expenses;

        NetTotal = income - expenses;



        FilterCalendarTransactions();

    }



    private void FilterCalendarTransactions()

    {

        CalendarTransactions.Clear();



        DateTime targetDate = SelectedCalendarDate?.Date ?? TimeService.Today;



        foreach (TransactionItemViewModel item in _items)

        {

            if (item.DateGroup == targetDate)

                CalendarTransactions.Add(new TransactionItemViewModel(item.SourceTransaction, _usdRate, _globalIsUsd));

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



        Transaction transaction = new Transaction

        {

            Type = dialog.TransactionType,
            Amount = dialog.Amount,
            Currency = dialog.Currency,
            Date = dialog.TransactionDate,
            Description = dialog.Description,
            AccountId = dialog.AccountId,
            CategoryId = dialog.CategoryId,
            SubCategoryId = dialog.SubCategoryId,
            FromAccountId = dialog.FromAccountId,
            ToAccountId = dialog.ToAccountId,
            ExchangeRate = _usdRate

        };

        dbContext.Transactions.Add(transaction);

        // Handle transfer: update account balances
        if (dialog.TransactionType == TransactionType.Transfer && dialog.FromAccountId.HasValue && dialog.ToAccountId.HasValue)
        {
            Account? fromAccount = dbContext.Accounts.Find(dialog.FromAccountId.Value);
            Account? toAccount = dbContext.Accounts.Find(dialog.ToAccountId.Value);

            if (fromAccount != null && toAccount != null)
            {
                // Convert amount if currencies differ
                decimal fromAmount = dialog.Amount;
                decimal toAmount = dialog.Amount;

                if (fromAccount.Currency != dialog.Currency)
                {
                    // Simple conversion: if account is SYP and transaction is USD, multiply by rate
                    // If account is USD and transaction is SYP, divide by rate
                    fromAmount = dialog.Currency == "USD" ? dialog.Amount * _usdRate : dialog.Amount / _usdRate;
                }

                if (toAccount.Currency != dialog.Currency)
                {
                    toAmount = dialog.Currency == "USD" ? dialog.Amount * _usdRate : dialog.Amount / _usdRate;
                }

                fromAccount.Balance -= fromAmount;
                toAccount.Balance += toAmount;
            }
        }
        else if (dialog.TransactionType == TransactionType.Income && dialog.AccountId.HasValue)
        {
            // Handle income: add to account balance
            Account? account = dbContext.Accounts.Find(dialog.AccountId.Value);
            if (account != null)
            {
                decimal amount = dialog.Amount;
                if (account.Currency != dialog.Currency)
                {
                    amount = dialog.Currency == "USD" ? dialog.Amount * _usdRate : dialog.Amount / _usdRate;
                }
                account.Balance += amount;
            }
        }
        else if (dialog.TransactionType == TransactionType.Expense && dialog.AccountId.HasValue)
        {
            // Handle expense: subtract from account balance
            Account? account = dbContext.Accounts.Find(dialog.AccountId.Value);
            if (account != null)
            {
                decimal amount = dialog.Amount;
                if (account.Currency != dialog.Currency)
                {
                    amount = dialog.Currency == "USD" ? dialog.Amount * _usdRate : dialog.Amount / _usdRate;
                }
                account.Balance -= amount;
            }
        }

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



    public void RefreshAmounts()

    {

        LoadTransactions();

    }

}



public partial class TransactionItemViewModel : ObservableObject

{

    private readonly Transaction transaction;

    private readonly decimal _usdRate;

    private readonly bool _globalIsUsd;



    public TransactionItemViewModel(Transaction transaction, decimal usdRate, bool globalIsUsd)

    {

        this.transaction = transaction;

        _usdRate = usdRate;

        _globalIsUsd = globalIsUsd;

        // Display amount in the transaction's original currency
        DisplayAmount = transaction.Amount;

    }



    public Transaction SourceTransaction => transaction;

    public decimal DisplayAmount { get; }



    public DateTime DateGroup => transaction.Date.Date;



    public string MonthGroup =>

        transaction.Date.ToString("MMMM yyyy", CultureInfo.CurrentUICulture);



    public string TimeText => transaction.Date.ToString("HH:mm");



    public string Description =>

        string.IsNullOrWhiteSpace(transaction.Description)

            ? AppUiResources.GetString("NoDescriptionText")

            : transaction.Description;



    public decimal Amount => transaction.Amount;

    public bool IsIncome => transaction.Type == TransactionType.Income;

    public bool IsTransfer => transaction.Type == TransactionType.Transfer;



    public string CategoryName =>
        transaction.Type == TransactionType.Transfer
            ? "تحويل"
            : transaction.SubCategory != null
                ? $"{transaction.Category?.Name ?? ""} - {transaction.SubCategory.Name}"
                : transaction.Category?.Name ?? AppUiResources.GetString("UncategorizedText");

    public string AccountName =>
        transaction.Type == TransactionType.Transfer
            ? $"{transaction.FromAccount?.Name ?? "؟"} → {transaction.ToAccount?.Name ?? "؟"}"
            : transaction.Account?.Name ?? AppUiResources.GetString("NoAccountText");



    public string GlobalSymbol => _globalIsUsd ? "$" : "ل.س";

    public string TransactionSymbol => transaction.Currency == "USD" ? "$" : "ل.س";

    public string FormattedAmount =>

        $"{NumberFormatting.Format(DisplayAmount, "N2")} {TransactionSymbol}";



    public void RefreshLanguage()

    {

        OnPropertyChanged(nameof(Description));

        OnPropertyChanged(nameof(CategoryName));

        OnPropertyChanged(nameof(AccountName));

        OnPropertyChanged(nameof(MonthGroup));

    }



    public void RefreshAmount()

    {

        OnPropertyChanged(nameof(FormattedAmount));

    }

}

