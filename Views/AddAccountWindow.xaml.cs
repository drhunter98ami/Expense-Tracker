using System.Windows;
using System.Windows.Controls;
using ExpenseTracker.Models;
using ExpenseTracker.Services;

namespace ExpenseTracker.Views;

public partial class AddAccountWindow : Window
{
    public string AccountName { get; private set; } = string.Empty;

    public string AccountGroup { get; private set; } = "Cash";

    public decimal InitialBalance { get; private set; }

    public int? CategoryId { get; private set; }

    public AddAccountWindow(List<string> existingGroups)
    {
        InitializeComponent();
        AppUiResources.ApplyToWindow(this);

        foreach (string group in existingGroups)
        {
            AccountGroupComboBox.Items.Add(group);
        }

        AccountGroupComboBox.SelectedIndex = 0;

        LoadIncomeCategories();
        InitialBalanceTextBox.Text = "0";
        AccountNameTextBox.Focus();
        UpdateCategoryState();
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
        {
            CategoryComboBox.SelectedIndex = 0;
        }
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
        {
            DialogResult = true;
        }
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

        string accountGroup = (AccountGroupComboBox.Text ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(accountGroup))
        {
            MessageBox.Show(
                AppUiResources.GetString("InvalidAccountGroupMessage"),
                AppUiResources.GetString("InvalidDataTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            AccountGroupComboBox.Focus();
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
        AccountGroup = accountGroup;
        InitialBalance = initialBalance;
        CategoryId = categoryId;

        return true;
    }
}
