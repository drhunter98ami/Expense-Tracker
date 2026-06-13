using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Models;
using ExpenseTracker.Services;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private string newIncomeCategoryName = string.Empty;

    [ObservableProperty]
    private string newExpenseCategoryName = string.Empty;

    [ObservableProperty]
    private string usdToSypRateText = string.Empty;

    public ObservableCollection<CategoryItemViewModel> IncomeCategories { get; } = [];

    public ObservableCollection<CategoryItemViewModel> ExpenseCategories { get; } = [];

    public SettingsViewModel()
    {
        LoadSettings();
    }

    [RelayCommand]
    private void AddIncomeCategory()
    {
        if (!TryCreateCategory(NewIncomeCategoryName, CategoryType.Income))
        {
            return;
        }

        NewIncomeCategoryName = string.Empty;
        LoadCategories();
    }

    [RelayCommand]
    private void AddExpenseCategory()
    {
        if (!TryCreateCategory(NewExpenseCategoryName, CategoryType.Expense))
        {
            return;
        }

        NewExpenseCategoryName = string.Empty;
        LoadCategories();
    }

    [RelayCommand]
    private void SaveExchangeRate()
    {
        if (!NumberFormatting.TryParseDecimal(UsdToSypRateText, out decimal rate) || rate <= 0)
        {
            MessageBox.Show(
                AppUiResources.GetString("InvalidExchangeRateMessage"),
                AppUiResources.GetString("InvalidDataTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        AppSettingsService.SaveUsdToSypRate(rate);
        UsdToSypRateText = NumberFormatting.Format(rate, "N2");
    }

    public void RefreshLanguage()
    {
        OnPropertyChanged(nameof(IncomeCategories));
        OnPropertyChanged(nameof(ExpenseCategories));
    }

    private void LoadSettings()
    {
        AppSetting settings = AppSettingsService.GetOrCreate();
        UsdToSypRateText = NumberFormatting.Format(settings.UsdToSypRate, "N2");
        LoadCategories();
    }

    private void LoadCategories()
    {
        using AppDbContext dbContext = new();

        List<Category> categories = dbContext.Categories
            .OrderBy(category => category.Name)
            .ToList();

        IncomeCategories.Clear();
        ExpenseCategories.Clear();

        foreach (Category category in categories)
        {
            CategoryItemViewModel item = new(category.Id, category.Name);

            if (category.Type == CategoryType.Income)
            {
                IncomeCategories.Add(item);
            }
            else
            {
                ExpenseCategories.Add(item);
            }
        }
    }

    private static bool TryCreateCategory(string name, CategoryType type)
    {
        string trimmedName = name.Trim();

        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            MessageBox.Show(
                AppUiResources.GetString("InvalidCategoryNameMessage"),
                AppUiResources.GetString("InvalidDataTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        using AppDbContext dbContext = new();

        bool alreadyExists = dbContext.Categories
            .Any(category =>
                category.Type == type &&
                category.Name.ToLower() == trimmedName.ToLower());

        if (alreadyExists)
        {
            MessageBox.Show(
                AppUiResources.GetString("DuplicateCategoryMessage"),
                AppUiResources.GetString("InvalidDataTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        dbContext.Categories.Add(new Category
        {
            Name = trimmedName,
            Type = type
        });

        dbContext.SaveChanges();
        return true;
    }
}

public class CategoryItemViewModel
{
    public CategoryItemViewModel(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; }

    public string Name { get; }
}
