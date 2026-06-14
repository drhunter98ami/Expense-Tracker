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
            Group = dialog.AccountGroup
        };

        if (dialog.InitialBalance > 0 && dialog.CategoryId is int categoryId)
        {
            account.Transactions.Add(new Transaction
            {
                Amount = dialog.InitialBalance,
                Date = DateTime.Today,
                CategoryId = categoryId,
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
                a.Group))
            .ToList();

        string[] groupOrder = ["Cash", "Savings"];

        List<AccountGroupViewModel> groups = accountItems
            .GroupBy(a => a.Group)
            .OrderBy(g => Array.IndexOf(groupOrder, g.Key) is int i && i >= 0 ? i : int.MaxValue)
            .ThenBy(g => g.Key)
            .Select(g => new AccountGroupViewModel(g.Key, GetGroupDisplayName(g.Key), g))
            .ToList();

        AccountGroups = new ObservableCollection<AccountGroupViewModel>(groups);
        TotalAssets = accountItems.Sum(a => a.Balance);
    }

    private static decimal CalculateBalance(IEnumerable<Transaction> transactions)
    {
        return transactions.Sum(t =>
            t.Category.Type == CategoryType.Income
                ? t.Amount
                : -t.Amount);
    }
}

public class AccountGroupViewModel
{
    public string GroupName { get; }
    public string GroupDisplayName { get; }
    public ObservableCollection<AccountItemViewModel> Accounts { get; }
    public decimal GroupTotal { get; }

    public AccountGroupViewModel(string groupName, string groupDisplayName, IEnumerable<AccountItemViewModel> accounts)
    {
        GroupName = groupName;
        GroupDisplayName = groupDisplayName;
        Accounts = new ObservableCollection<AccountItemViewModel>(accounts);
        GroupTotal = Accounts.Sum(a => a.Balance);
    }
}

public class AccountItemViewModel
{
    public AccountItemViewModel(string name, decimal balance, string group)
    {
        Name = name;
        Balance = balance;
        Group = group;
    }

    public string Name { get; }
    public decimal Balance { get; }
    public string Group { get; }
}
