using System.Windows;
using ExpenseTracker.Services;
using ExpenseTracker.Models;
using ExpenseTracker.Services;

namespace ExpenseTracker.Views;

public partial class AddTransactionWindow : Window
{
    public TransactionType TransactionType { get; private set; }

    public decimal Amount { get; private set; }

    public string Currency { get; private set; } = "SYP";

    public DateTime TransactionDate { get; private set; }

    public string? Description { get; private set; }

    public int? AccountId { get; private set; }

    public int? CategoryId { get; private set; }

    public int? FromAccountId { get; private set; }

    public int? ToAccountId { get; private set; }

    public AddTransactionWindow()
    {
        InitializeComponent();
        AppUiResources.ApplyToWindow(this);
        LoadLookups();
        TransactionDatePicker.SelectedDate = TimeService.Today;
        TimeTextBox.Text = TimeService.Now.ToString("HH:mm");
        AmountTextBox.TextChanged += AmountTextBox_TextChanged;
        TransactionTypeComboBox.SelectedIndex = 0;
        CurrencyComboBox.SelectedIndex = 0;
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
        FromComboBox.ItemsSource = accounts;
        ToComboBox.ItemsSource = accounts;

        if (accounts.Count > 0)
        {
            AccountComboBox.SelectedIndex = 0;
            FromComboBox.SelectedIndex = 0;
            ToComboBox.SelectedIndex = Math.Min(1, accounts.Count - 1);
        }

        if (categories.Count > 0)
        {
            CategoryComboBox.SelectedIndex = 0;
        }
    }

    private void TransactionTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (TransactionTypeComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem)
        {
            string tag = selectedItem.Tag?.ToString() ?? "Income";
            
            if (tag == "Transfer")
            {
                AccountCategoryPanel.Visibility = System.Windows.Visibility.Collapsed;
                CategoryPanel.Visibility = System.Windows.Visibility.Collapsed;
                FromPanel.Visibility = System.Windows.Visibility.Visible;
                ToPanel.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                AccountCategoryPanel.Visibility = System.Windows.Visibility.Visible;
                CategoryPanel.Visibility = System.Windows.Visibility.Visible;
                FromPanel.Visibility = System.Windows.Visibility.Collapsed;
                ToPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
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
        if (TransactionTypeComboBox.SelectedItem is not System.Windows.Controls.ComboBoxItem typeItem)
        {
            MessageBox.Show("الرجاء اختيار نوع المعاملة", "بيانات غير صحيحة", MessageBoxButton.OK, MessageBoxImage.Warning);
            TransactionTypeComboBox.Focus();
            return false;
        }

        string typeTag = typeItem.Tag?.ToString() ?? "Income";
        TransactionType = typeTag switch
        {
            "Income" => Models.TransactionType.Income,
            "Expense" => Models.TransactionType.Expense,
            "Transfer" => Models.TransactionType.Transfer,
            _ => Models.TransactionType.Income
        };

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

        if (!TimeSpan.TryParse(TimeTextBox.Text, out TimeSpan time))
        {
            MessageBox.Show("الرجاء إدخال وقت صحيح (مثال: 14:30)", "بيانات غير صحيحة", MessageBoxButton.OK, MessageBoxImage.Warning);
            TimeTextBox.Focus();
            return false;
        }

        if (CurrencyComboBox.SelectedItem is not System.Windows.Controls.ComboBoxItem currencyItem)
        {
            MessageBox.Show("الرجاء اختيار العملة", "بيانات غير صحيحة", MessageBoxButton.OK, MessageBoxImage.Warning);
            CurrencyComboBox.Focus();
            return false;
        }

        Currency = currencyItem.Tag?.ToString() ?? "SYP";

        if (TransactionType == Models.TransactionType.Transfer)
        {
            if (FromComboBox.SelectedValue is not int fromAccountId)
            {
                MessageBox.Show("الرجاء اختيار الحساب المصدر", "بيانات غير صحيحة", MessageBoxButton.OK, MessageBoxImage.Warning);
                FromComboBox.Focus();
                return false;
            }

            if (ToComboBox.SelectedValue is not int toAccountId)
            {
                MessageBox.Show("الرجاء اختيار الحساب المستهدف", "بيانات غير صحيحة", MessageBoxButton.OK, MessageBoxImage.Warning);
                ToComboBox.Focus();
                return false;
            }

            if (fromAccountId == toAccountId)
            {
                MessageBox.Show("لا يمكن التحويل إلى نفس الحساب", "بيانات غير صحيحة", MessageBoxButton.OK, MessageBoxImage.Warning);
                ToComboBox.Focus();
                return false;
            }

            FromAccountId = fromAccountId;
            ToAccountId = toAccountId;
            AccountId = null;
            CategoryId = null;
        }
        else
        {
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

            AccountId = selectedAccountId;
            CategoryId = selectedCategoryId;
            FromAccountId = null;
            ToAccountId = null;
        }

        Amount = amount;
        TransactionDate = selectedDate.Date.Add(time);
        Description = string.IsNullOrWhiteSpace(DescriptionTextBox.Text)
            ? null
            : DescriptionTextBox.Text.Trim();

        return true;
    }
}
