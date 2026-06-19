using System.Windows;
using System.Windows.Controls;
using ExpenseTracker.Services;
using ExpenseTracker.ViewModels;
using ExpenseTracker.Views;

namespace ExpenseTracker;

public partial class MainWindow : Window
{
    private readonly MainViewModel viewModel;
    private readonly TransactionsView _transactionsView = new();
    private readonly StatsView _statsView = new();
    private readonly AccountsView _accountsView = new();
    private readonly SettingsView _settingsView = new();
    public ClockViewModel ClockViewModel { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        viewModel = new MainViewModel();
        DataContext = viewModel;
        AppUiResources.ApplyToWindow(this);
        NavigateTo(_transactionsView, viewModel.CurrentViewModel, TransactionsButton);
    }

    private void TransactionsButton_Click(object sender, RoutedEventArgs e)
    {
        viewModel.ShowTransactionsCommand.Execute(null);
        NavigateTo(_transactionsView, viewModel.CurrentViewModel, TransactionsButton);
    }

    private void StatsButton_Click(object sender, RoutedEventArgs e)
    {
        viewModel.ShowStatsCommand.Execute(null);
        NavigateTo(_statsView, viewModel.CurrentViewModel, StatsButton);
    }

    private void AccountsButton_Click(object sender, RoutedEventArgs e)
    {
        viewModel.ShowAccountsCommand.Execute(null);
        NavigateTo(_accountsView, viewModel.CurrentViewModel, AccountsButton);
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        viewModel.ShowSettingsCommand.Execute(null);
        NavigateTo(_settingsView, viewModel.CurrentViewModel, SettingsButton);
    }

    private void DeleteAllDataButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
            AppUiResources.GetString("DeleteAllDataConfirmMessage"),
            AppUiResources.GetString("DeleteAllDataConfirmTitle"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        using AppDbContext dbContext = new();
        dbContext.Transactions.RemoveRange(dbContext.Transactions);
        dbContext.Accounts.RemoveRange(dbContext.Accounts);
        dbContext.Categories.RemoveRange(dbContext.Categories);
        dbContext.SaveChanges();

        viewModel.ShowTransactionsCommand.Execute(null);
        NavigateTo(_transactionsView, viewModel.CurrentViewModel, TransactionsButton);
    }

    private void NavigateTo(UserControl view, object currentViewModel, Button activeButton)
    {
        view.DataContext = currentViewModel;
        MainContent.Content = view;
        SetActiveNavButton(activeButton);
    }

    private void SetActiveNavButton(Button activeButton)
    {
        TransactionsButton.Tag = null;
        StatsButton.Tag = null;
        AccountsButton.Tag = null;
        SettingsButton.Tag = null;
        activeButton.Tag = "Active";
    }
}
