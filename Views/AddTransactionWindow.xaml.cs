using System.Windows;
using ExpenseTracker.Services;
using ExpenseTracker.Models;
using ExpenseTracker.Services;

namespace ExpenseTracker.Views;

public partial class AddTransactionWindow : Window
{
    public decimal Amount { get; private set; }

    public DateTime TransactionDate { get; private set; }

    public string? Description { get; private set; }

    public int AccountId { get; private set; }

    public int CategoryId { get; private set; }

    public AddTransactionWindow()
    {
        InitializeComponent();
        AppUiResources.ApplyToWindow(this);
        LoadLookups();
        TransactionDatePicker.SelectedDate = TimeService.Today;
        AmountTextBox.TextChanged += AmountTextBox_TextChanged;
    }

    private void AmountTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (sender is System.Windows.Controls.TextBox textBox)
        {
            string normalized = NumberFormatting.NormalizeDigits(textBox.Text);

            if (normalized != textBox.Text)
            {
                int caretIndex = textBox.CaretIndex;
                textBox.Text = normalized;
                textBox.CaretIndex = Math.Min(caretIndex, textBox.Text.Length);
            }
        }
    }

    private void LoadLookups()
    {
        using AppDbContext dbContext = new();

        List<Account> accounts = dbContext.Accounts
            .OrderBy(account => account.Name)
            .ToList();

        List<Category> categories = dbContext.Categories
            .OrderBy(category => category.Name)
            .ToList();

        AccountComboBox.ItemsSource = accounts;
        CategoryComboBox.ItemsSource = categories;

        if (accounts.Count > 0)
        {
            AccountComboBox.SelectedIndex = 0;
        }

        if (categories.Count > 0)
        {
            CategoryComboBox.SelectedIndex = 0;
        }
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
        if (!NumberFormatting.TryParseDecimal(AmountTextBox.Text, out decimal amount) || amount <= 0)
        {
            MessageBox.Show(
                AppUiResources.GetString("InvalidAmountMessage"),
                AppUiResources.GetString("InvalidDataTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            AmountTextBox.Focus();
            return false;
        }

        if (TransactionDatePicker.SelectedDate is not DateTime selectedDate)
        {
            MessageBox.Show(
                AppUiResources.GetString("MissingDateMessage"),
                AppUiResources.GetString("InvalidDataTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            TransactionDatePicker.Focus();
            return false;
        }

        if (AccountComboBox.SelectedValue is not int selectedAccountId)
        {
            MessageBox.Show(
                AppUiResources.GetString("MissingAccountMessage"),
                AppUiResources.GetString("InvalidDataTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            AccountComboBox.Focus();
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

        Amount = amount;
        TransactionDate = selectedDate.Date.Add(TimeService.Now.TimeOfDay);
        Description = string.IsNullOrWhiteSpace(DescriptionTextBox.Text)
            ? null
            : DescriptionTextBox.Text.Trim();
        AccountId = selectedAccountId;
        CategoryId = selectedCategoryId;

        return true;
    }
}
