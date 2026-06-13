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
    private ObservableCollection<AccountItemViewModel> accounts = [];

    [ObservableProperty]
    private decimal totalNetWorth;

    public AccountsViewModel()
    {
        LoadAccounts();
    }

    [RelayCommand]
    private void AddAccount()
    {
        AddAccountWindow dialog = new();

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
            Name = dialog.AccountName
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
        OnPropertyChanged(nameof(Accounts));
        OnPropertyChanged(nameof(TotalNetWorth));
    }

    private void LoadAccounts()
    {
        using AppDbContext dbContext = new();

        List<AccountItemViewModel> accountItems = dbContext.Accounts
            .Include(account => account.Transactions)
            .ThenInclude(transaction => transaction.Category)
            .OrderBy(account => account.Name)
            .AsEnumerable()
            .Select(account => new AccountItemViewModel(
                account.Name,
                CalculateBalance(account.Transactions)))
            .ToList();

        Accounts = new ObservableCollection<AccountItemViewModel>(accountItems);
        TotalNetWorth = accountItems.Sum(account => account.Balance);
    }

    private static decimal CalculateBalance(IEnumerable<Transaction> transactions)
    {
        return transactions.Sum(transaction =>
            transaction.Category.Type == CategoryType.Income
                ? transaction.Amount
                : -transaction.Amount);
    }
}

public class AccountItemViewModel
{
    public AccountItemViewModel(string name, decimal balance)
    {
        Name = name;
        Balance = balance;
    }

    public string Name { get; }

    public decimal Balance { get; }
}
