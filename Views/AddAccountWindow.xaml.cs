using System.Windows;
using System.Windows.Controls;
using ExpenseTracker.Models;
using ExpenseTracker.Services;

namespace ExpenseTracker.Views;

public partial class AddAccountWindow : Window
{
    public string AccountName { get; private set; } = string.Empty;

    public decimal InitialBalance { get; private set; }

    public int? CategoryId { get; private set; }

    public AddAccountWindow()
    {
        InitializeComponent();
        AppUiResources.ApplyToWindow(this);
        LoadCategories();
        InitialBalanceTextBox.Text = "0";
        AccountNameTextBox.Focus();
        UpdateCategoryState();
    }

    private void LoadCategories()
    {
        using AppDbContext dbContext = new();

        List<Category> categories = dbContext.Categories
            .Where(category => category.Type == CategoryType.Income)
            .OrderBy(category => category.Name)
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
        CategoryComboBox.Opacity = requiresCategory ? 1 : 0.55;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryReadForm())
        {
            return;
        }

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
        InitialBalance = initialBalance;
        CategoryId = categoryId;

        return true;
    }
}
