using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Models;
using ExpenseTracker.Services;
using ExpenseTracker.Views;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.ViewModels;

public partial class AccountsViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<AccountGroupViewModel> accountGroups = [];

    [ObservableProperty]
    private decimal totalAssets;

    [ObservableProperty]
    private string totalAssetsSymbol = "ل.س";

    public AccountsViewModel()
    {
        LoadAccounts();
    }

    [RelayCommand]
    private void AddAccount()
    {
        List<string> customGroups = LoadCustomGroups();
        AddAccountWindow dialog = new(customGroups);

        if (Application.Current.MainWindow is Window owner)
            dialog.Owner = owner;

        if (dialog.ShowDialog() != true)
            return;

        using AppDbContext dbContext = new();

        Account account = new()
        {
            Name = dialog.AccountName,
            Group = dialog.AccountGroup,
            Currency = dialog.AccountCurrency
        };

        if (dialog.InitialBalance > 0)
        {
            Category? incomeCategory = dbContext.Categories
                .Where(c => c.Type == CategoryType.Income)
                .OrderBy(c => c.Name)
                .FirstOrDefault();

            if (incomeCategory is null)
            {
                incomeCategory = new Category
                {
                    Name = AppUiResources.GetString("InitialBalanceDescription"),
                    Type = CategoryType.Income
                };
                dbContext.Categories.Add(incomeCategory);
                dbContext.SaveChanges();
            }

            account.Transactions.Add(new Transaction
            {
                Amount = dialog.InitialBalance,
                Date = TimeService.Now,
                CategoryId = incomeCategory.Id,
                Description = AppUiResources.GetString("InitialBalanceDescription")
            });
        }

        dbContext.Accounts.Add(account);
        dbContext.SaveChanges();
        LoadAccounts();
    }

    public void RefreshLanguage()
    {
        LoadAccounts();
    }

    public void RefreshAmounts()
    {
        LoadAccounts();
    }

    private static List<string> LoadCustomGroups()
    {
        using AppDbContext dbContext = new();
        string[] builtIn = ["Cash", "Savings"];

        return dbContext.Accounts
            .Select(a => a.Group)
            .Distinct()
            .AsEnumerable()
            .Where(g => !builtIn.Contains(g))
            .OrderBy(g => g)
            .ToList();
    }

    private static string GetGroupDisplayName(string key) => key switch
    {
        "Cash" => AppUiResources.GetString("CashGroupName"),
        "Savings" => AppUiResources.GetString("SavingsGroupName"),
        _ => key
    };

    private void LoadAccounts()
    {
        AppSetting settings = AppSettingsService.GetOrCreate();
        decimal usdRate = settings.UsdToSypRate > 0 ? settings.UsdToSypRate : 1;
        bool globalIsUsd = settings.CurrencyCode == "USD";
        string globalSymbol = globalIsUsd ? "$" : "ل.س";

        using AppDbContext dbContext = new();

        List<AccountItemViewModel> accountItems = dbContext.Accounts
            .Include(a => a.Transactions)
            .ThenInclude(t => t.Category)
            .OrderBy(a => a.Group)
            .ThenBy(a => a.Name)
            .AsEnumerable()
            .Select(a => new AccountItemViewModel(
                a.Name,
                CalculateBalance(a.Transactions),
                a.Group,
                a.Currency,
                usdRate,
                globalIsUsd))
            .ToList();

        string[] groupOrder = ["Cash", "Savings"];

        List<AccountGroupViewModel> groups = accountItems
            .GroupBy(a => a.Group)
            .OrderBy(g => Array.IndexOf(groupOrder, g.Key) is int i && i >= 0 ? i : int.MaxValue)
            .ThenBy(g => g.Key)
            .Select(g => new AccountGroupViewModel(g.Key, GetGroupDisplayName(g.Key), g, globalSymbol))
            .ToList();

        AccountGroups = new ObservableCollection<AccountGroupViewModel>(groups);
        TotalAssets = accountItems.Sum(a => a.DisplayBalance);
        TotalAssetsSymbol = globalSymbol;
    }

    private static decimal CalculateBalance(IEnumerable<Transaction> transactions)
    {
        return transactions.Sum(t =>
            t.Category.Type == CategoryType.Income ? t.Amount : -t.Amount);
    }
}

public class AccountGroupViewModel
{
    public string GroupName { get; }
    public string GroupDisplayName { get; }
    public ObservableCollection<AccountItemViewModel> Accounts { get; }
    public decimal GroupTotal { get; }
    public string GroupSymbol { get; }

    public string FormattedGroupTotal =>
        $"{NumberFormatting.Format(GroupTotal, "N2")} {GroupSymbol}";

    public AccountGroupViewModel(string groupName, string groupDisplayName,
        IEnumerable<AccountItemViewModel> accounts, string globalSymbol)
    {
        GroupName = groupName;
        GroupDisplayName = groupDisplayName;
        Accounts = new ObservableCollection<AccountItemViewModel>(accounts);
        GroupTotal = Accounts.Sum(a => a.DisplayBalance);
        GroupSymbol = globalSymbol;
    }
}

public class AccountItemViewModel
{
    public AccountItemViewModel(string name, decimal balance, string group,
        string currency, decimal usdRate, bool globalIsUsd)
    {
        Name = name;
        Balance = balance;
        Group = group;
        Currency = currency;

        // Individual accounts always show their original balance in their own currency
        LocalSymbol = currency == "USD" ? "$" : "ل.س";

        // Calculate display balance for totals (converted to global currency)
        bool isUsdAccount = currency == "USD";
        if (isUsdAccount)
            DisplayBalance = globalIsUsd ? balance : balance * usdRate;
        else
            DisplayBalance = globalIsUsd ? balance / usdRate : balance;
    }

    public string Name { get; }
    public decimal Balance { get; }
    public string Group { get; }
    public string Currency { get; }
    public string LocalSymbol { get; }
    public decimal DisplayBalance { get; }

    public string FormattedBalance =>
        $"{NumberFormatting.Format(Balance, "N2")} {LocalSymbol}";
}
