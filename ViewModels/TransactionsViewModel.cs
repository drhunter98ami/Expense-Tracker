using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Models;
using ExpenseTracker.Services;
using ExpenseTracker.Views;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.ViewModels;

public partial class TransactionsViewModel
{
    public ObservableCollection<TransactionItemViewModel> Transactions { get; } = [];

    public ICollectionView GroupedTransactions { get; }

    public TransactionsViewModel()
    {
        GroupedTransactions = CollectionViewSource.GetDefaultView(Transactions);
        GroupedTransactions.GroupDescriptions.Add(
            new PropertyGroupDescription(nameof(TransactionItemViewModel.DateGroup)));

        LoadTransactions();
    }

    private void LoadTransactions()
    {
        Transactions.Clear();

        using AppDbContext dbContext = new();

        List<Transaction> transactions = dbContext.Transactions
            .Include(transaction => transaction.Account)
            .Include(transaction => transaction.Category)
            .OrderByDescending(transaction => transaction.Date)
            .ToList();

        foreach (Transaction transaction in transactions)
        {
            Transactions.Add(new TransactionItemViewModel(transaction));
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
        {
            dialog.Owner = owner;
        }

        if (dialog.ShowDialog() != true)
        {
            return;
        }

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
        foreach (TransactionItemViewModel transaction in Transactions)
        {
            transaction.RefreshLanguage();
        }

        GroupedTransactions.Refresh();
    }
}

public partial class TransactionItemViewModel : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
{
    private readonly Transaction transaction;

    public TransactionItemViewModel(Transaction transaction)
    {
        this.transaction = transaction;
    }

    public DateTime DateGroup => transaction.Date.Date;

    public string Description =>
        string.IsNullOrWhiteSpace(transaction.Description)
            ? AppUiResources.GetString("NoDescriptionText")
            : transaction.Description;

    public decimal Amount => transaction.Amount;

    public string CategoryName => transaction.Category?.Name ?? AppUiResources.GetString("UncategorizedText");

    public string AccountName => transaction.Account?.Name ?? AppUiResources.GetString("NoAccountText");

    public void RefreshLanguage()
    {
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(CategoryName));
        OnPropertyChanged(nameof(AccountName));
    }
}
