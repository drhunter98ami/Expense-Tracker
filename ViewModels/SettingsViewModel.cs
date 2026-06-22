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
    private string newSubCategoryName = string.Empty;

    [ObservableProperty]
    private int? selectedParentCategoryId;

    [ObservableProperty]
    private string usdToSypRateText = string.Empty;

    [ObservableProperty]
    private bool isUsd;

    public ObservableCollection<CategoryItemViewModel> IncomeCategories { get; } = [];
    public ObservableCollection<CategoryItemViewModel> ExpenseCategories { get; } = [];
    public ObservableCollection<CategoryItemViewModel> AllParentCategories { get; } = [];

    public SettingsViewModel()
    {
        LoadSettings();
    }

    partial void OnIsUsdChanged(bool value)
    {
        string code = value ? "USD" : "SYP";
        AppSettingsService.SaveCurrencyCode(code);
        AppUiResources.ApplyCurrencySymbol(code);
    }

    [RelayCommand]
    private void SelectSyp() => IsUsd = false;

    [RelayCommand]
    private void SelectUsd() => IsUsd = true;

    [RelayCommand]
    private void AddIncomeCategory()
    {
        if (!TryCreateCategory(NewIncomeCategoryName, CategoryType.Income))
            return;

        NewIncomeCategoryName = string.Empty;
        LoadCategories();
    }

    [RelayCommand]
    private void AddExpenseCategory()
    {
        if (!TryCreateCategory(NewExpenseCategoryName, CategoryType.Expense))
            return;

        NewExpenseCategoryName = string.Empty;
        LoadCategories();
    }

    [RelayCommand]
    private void AddSubCategory()
    {
        if (SelectedParentCategoryId == null)
        {
            MessageBox.Show(
                AppUiResources.GetString("SelectParentCategoryMessage"),
                AppUiResources.GetString("InvalidDataTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (!TryCreateSubCategory(NewSubCategoryName, SelectedParentCategoryId.Value))
            return;

        NewSubCategoryName = string.Empty;
        SelectedParentCategoryId = null;
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

        MessageBox.Show(
            AppUiResources.GetString("ExchangeRateSavedMessage"),
            AppUiResources.GetString("SuccessTitle"),
            MessageBoxButton.OK,
            MessageBoxImage.Information);

        AppUiResources.ApplyCurrencySymbol(IsUsd ? "USD" : "SYP");
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
        IsUsd = settings.CurrencyCode == "USD";
        LoadCategories();
    }

    private void LoadCategories()
    {
        using AppDbContext dbContext = new();

        List<Category> categories = dbContext.Categories
            .Include(c => c.SubCategories)
            .OrderBy(c => c.Name)
            .ToList();

        IncomeCategories.Clear();
        ExpenseCategories.Clear();
        AllParentCategories.Clear();

        Dictionary<int, CategoryItemViewModel> categoryMap = new();

        foreach (Category category in categories)
        {
            CategoryItemViewModel item = new(category.Id, category.Name, category.ParentCategoryId);
            categoryMap[category.Id] = item;

            if (category.ParentCategoryId == null)
            {
                if (category.Type == CategoryType.Income)
                {
                    IncomeCategories.Add(item);
                    AllParentCategories.Add(item);
                }
                else
                {
                    ExpenseCategories.Add(item);
                    AllParentCategories.Add(item);
                }
            }
        }

        foreach (Category category in categories)
        {
            if (category.ParentCategoryId != null && categoryMap.ContainsKey(category.ParentCategoryId.Value))
            {
                CategoryItemViewModel subItem = categoryMap[category.Id];
                CategoryItemViewModel parentItem = categoryMap[category.ParentCategoryId.Value];
                parentItem.SubCategories.Add(subItem);
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
            .Any(c => c.Type == type && c.Name.ToLower() == trimmedName.ToLower());

        if (alreadyExists)
        {
            MessageBox.Show(
                AppUiResources.GetString("DuplicateCategoryMessage"),
                AppUiResources.GetString("InvalidDataTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        dbContext.Categories.Add(new Category { Name = trimmedName, Type = type });
        dbContext.SaveChanges();
        return true;
    }

    private static bool TryCreateSubCategory(string name, int parentCategoryId)
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

        Category? parentCategory = dbContext.Categories.FirstOrDefault(c => c.Id == parentCategoryId);
        if (parentCategory == null)
        {
            MessageBox.Show(
                AppUiResources.GetString("ParentCategoryNotFoundMessage"),
                AppUiResources.GetString("InvalidDataTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        bool alreadyExists = dbContext.Categories
            .Any(c => c.ParentCategoryId == parentCategoryId && c.Name.ToLower() == trimmedName.ToLower());

        if (alreadyExists)
        {
            MessageBox.Show(
                AppUiResources.GetString("DuplicateSubCategoryMessage"),
                AppUiResources.GetString("InvalidDataTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        dbContext.Categories.Add(new Category
        {
            Name = trimmedName,
            Type = parentCategory.Type,
            ParentCategoryId = parentCategoryId
        });
        dbContext.SaveChanges();
        return true;
    }
}

public class CategoryItemViewModel
{
    public CategoryItemViewModel(int id, string name, int? parentCategoryId = null)
    {
        Id = id;
        Name = name;
        ParentCategoryId = parentCategoryId;
        SubCategories = [];
    }

    public int Id { get; }
    public string Name { get; }
    public int? ParentCategoryId { get; }
    public bool IsSubCategory => ParentCategoryId != null;
    public ObservableCollection<CategoryItemViewModel> SubCategories { get; }
}
