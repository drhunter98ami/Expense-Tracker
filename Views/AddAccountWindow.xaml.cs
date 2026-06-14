using System.Windows;
using System.Windows.Controls;
using ExpenseTracker.Models;
using ExpenseTracker.Services;

namespace ExpenseTracker.Views;

public class AccountGroupOption
{
    public string Key { get; }
    public string DisplayName { get; }

    public AccountGroupOption(string key, string displayName)
    {
        Key = key;
        DisplayName = displayName;
    }
}

public partial class AddAccountWindow : Window
{
    public string AccountName { get; private set; } = string.Empty;
    public string AccountGroup { get; private set; } = "Cash";
    public decimal InitialBalance { get; private set; }
    public int? CategoryId { get; private set; }

    public AddAccountWindow(List<string> existingCustomGroups)
    {
        InitializeComponent();
        AppUiResources.ApplyToWindow(this);

        PopulateGroupComboBox(existingCustomGroups);
        LoadIncomeCategories();

        InitialBalanceTextBox.Text = "0";
        AccountNameTextBox.Focus();
        UpdateCategoryState();
    }

    private void PopulateGroupComboBox(List<string> existingCustomGroups)
    {
        string cashLabel = AppUiResources.GetString("CashGroupName");
        string savingsLabel = AppUiResources.GetString("SavingsGroupName");

        AccountGroupComboBox.Items.Add(new AccountGroupOption("Cash", cashLabel));
        AccountGroupComboBox.Items.Add(new AccountGroupOption("Savings", savingsLabel));

        foreach (string custom in existingCustomGroups)
        {
            AccountGroupComboBox.Items.Add(new AccountGroupOption(custom, custom));
        }

        AccountGroupComboBox.SelectedIndex = 0;
    }

    private void LoadIncomeCategories()
    {
        using AppDbContext dbContext = new();

        List<Category> categories = dbContext.Categories
            .Where(c => c.Type == CategoryType.Income)
            .OrderBy(c => c.Name)
            .ToList();

        CategoryComboBox.ItemsSource = categories;

        if (categories.Count > 0)
            CategoryComboBox.SelectedIndex = 0;
    }

    private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
    {
        string newName = NewCategoryTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(newName))
            return;

        foreach (AccountGroupOption existing in AccountGroupComboBox.Items)
        {
            if (existing.Key.Equals(newName, StringComparison.OrdinalIgnoreCase))
            {
                AccountGroupComboBox.SelectedItem = existing;
                NewCategoryTextBox.Clear();
                return;
            }
        }

        AccountGroupOption newOption = new(newName, newName);
        AccountGroupComboBox.Items.Add(newOption);
        AccountGroupComboBox.SelectedItem = newOption;
        NewCategoryTextBox.Clear();
    }

    private void InitialBalanceTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            string normalized = NumberFormatting.NormalizeDigits(textBox.Text);

            if (normalized != textBox.Text)
            {
                int caretIndex = textBox.CaretIndex;
                textBox.Text = normalized;
                textBox.CaretIndex = Math.Min(caretIndex, textBox.Text.Length);
            }
        }

        UpdateCategoryState();
    }

    private void UpdateCategoryState()
    {
        bool requiresCategory = NumberFormatting.TryParseDecimal(InitialBalanceTextBox.Text, out decimal amount)
            && amount > 0;
        CategoryComboBox.IsEnabled = requiresCategory;
        CategoryComboBox.Opacity = requiresCategory ? 1 : 0.5;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (TryReadForm())
            DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private bool TryReadForm()
    {
        string accountName = AccountNameTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(accountName))
        {
            MessageBox.Show(
                AppUiResources.GetString("InvalidAccountNameMessage"),
                AppUiResources.GetString("InvalidDataTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            AccountNameTextBox.Focus();
            return false;
        }

        if (AccountGroupComboBox.SelectedItem is not AccountGroupOption selectedGroup)
        {
            MessageBox.Show(
                AppUiResources.GetString("InvalidAccountGroupMessage"),
                AppUiResources.GetString("InvalidDataTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        if (!NumberFormatting.TryParseDecimal(InitialBalanceTextBox.Text, out decimal initialBalance) || initialBalance < 0)
        {
            MessageBox.Show(
                AppUiResources.GetString("InvalidInitialBalanceMessage"),
                AppUiResources.GetString("InvalidDataTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            InitialBalanceTextBox.Focus();
            return false;
        }

        int? categoryId = null;

        if (initialBalance > 0)
        {
            if (CategoryComboBox.Items.Count == 0)
            {
                MessageBox.Show(
                    AppUiResources.GetString("MissingIncomeCategoryMessage"),
                    AppUiResources.GetString("InvalidDataTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return false;
            }

            if (CategoryComboBox.SelectedValue is not int selectedCategoryId)
            {
                MessageBox.Show(
                    AppUiResources.GetString("MissingCategoryMessage"),
                    AppUiResources.GetString("InvalidDataTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                CategoryComboBox.Focus();
                return false;
            }

            categoryId = selectedCategoryId;
        }

        AccountName = accountName;
        AccountGroup = selectedGroup.Key;
        InitialBalance = initialBalance;
        CategoryId = categoryId;

        return true;
    }
}
