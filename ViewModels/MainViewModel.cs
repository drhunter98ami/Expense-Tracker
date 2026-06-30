using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Services;

namespace ExpenseTracker.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private object currentViewModel;

    [ObservableProperty]
    private bool isEnglish;

    [ObservableProperty]
    private bool isDarkMode;

    public MainViewModel()
    {
        AppUiResources.CurrencySymbolChanged += RefreshCurrentViewModelAmounts;

        var settings = AppSettingsService.GetOrCreate();
        isEnglish = settings.IsEnglish;
        isDarkMode = settings.IsDarkMode;

        AppUiResources.Apply(isEnglish, isDarkMode);
        currentViewModel = new TransactionsViewModel();
    }

    partial void OnIsEnglishChanged(bool value)
    {
        AppUiResources.Apply(value, IsDarkMode);
        RefreshCurrentViewModelLanguage();
        AppSettingsService.SaveLanguage(value);
    }

    partial void OnIsDarkModeChanged(bool value)
    {
        AppUiResources.Apply(IsEnglish, value);
        AppSettingsService.SaveTheme(value);
    }

    [RelayCommand]
    private void ShowTransactions()
    {
        CurrentViewModel = new TransactionsViewModel();
    }

    [RelayCommand]
    private void ShowStats()
    {
        CurrentViewModel = new StatsViewModel();
    }

    [RelayCommand]
    private void ShowAccounts()
    {
        CurrentViewModel = new AccountsViewModel();
    }

    [RelayCommand]
    private void ShowSettings()
    {
        CurrentViewModel = new SettingsViewModel();
    }

    private void RefreshCurrentViewModelLanguage()
    {
        if (CurrentViewModel is TransactionsViewModel transactionsViewModel)
            transactionsViewModel.RefreshLanguage();

        if (CurrentViewModel is AccountsViewModel accountsViewModel)
            accountsViewModel.RefreshLanguage();

        if (CurrentViewModel is SettingsViewModel settingsViewModel)
            settingsViewModel.RefreshLanguage();
    }

    private void RefreshCurrentViewModelAmounts()
    {
        if (CurrentViewModel is TransactionsViewModel transactionsViewModel)
            transactionsViewModel.RefreshAmounts();

        if (CurrentViewModel is AccountsViewModel accountsViewModel)
            accountsViewModel.RefreshAmounts();
    }
}
