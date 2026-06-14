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
        List<string> existingGroups = LoadExistingGroups();
        AddAccountWindow dialog = new(existingGroups);

        if (Application.Current.MainWindow is Window owner)
        {
            dialog.Owner = owner;
        }

        if (dialog.ShowDialog() != true)
        {
            return;
        }

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
        OnPropertyChanged(nameof(AccountGroups));
        OnPropertyChanged(nameof(TotalAssets));
    }

    private static List<string> LoadExistingGroups()
    {
        using AppDbContext dbContext = new();

        List<string> dbGroups = dbContext.Accounts
            .Select(a => a.Group)
            .Distinct()
            .OrderBy(g => g)
            .ToList();

        List<string> defaults = ["Cash", "Savings"];

        return defaults
            .Concat(dbGroups.Where(g => !defaults.Contains(g)))
            .ToList();
    }

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

        List<AccountGroupViewModel> groups = accountItems
            .GroupBy(a => a.Group)
            .Select(g => new AccountGroupViewModel(g.Key, g))
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
    public ObservableCollection<AccountItemViewModel> Accounts { get; }
    public decimal GroupTotal { get; }

    public AccountGroupViewModel(string groupName, IEnumerable<AccountItemViewModel> accounts)
    {
        GroupName = groupName;
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
