using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
    private string _accountCurrency = "SYP";

    public string AccountName { get; private set; } = string.Empty;
    public string AccountGroup { get; private set; } = "Cash";
    public string AccountCurrency { get; private set; } = "SYP";
    public decimal InitialBalance { get; private set; }

    public AddAccountWindow(List<string> existingCustomGroups)
    {
        InitializeComponent();
        AppUiResources.ApplyToWindow(this);
        PopulateGroupComboBox(existingCustomGroups);
        InitialBalanceTextBox.Text = "0";
        AccountNameTextBox.Focus();
        UpdateCurrencyButtons();
    }

    private void PopulateGroupComboBox(List<string> existingCustomGroups)
    {
        string cashLabel = AppUiResources.GetString("CashGroupName");
        string savingsLabel = AppUiResources.GetString("SavingsGroupName");

        AccountGroupComboBox.Items.Add(new AccountGroupOption("Cash", cashLabel));
        AccountGroupComboBox.Items.Add(new AccountGroupOption("Savings", savingsLabel));

        foreach (string custom in existingCustomGroups)
            AccountGroupComboBox.Items.Add(new AccountGroupOption(custom, custom));

        AccountGroupComboBox.SelectedIndex = 0;
    }

    private void SypButton_Click(object sender, RoutedEventArgs e)
    {
        _accountCurrency = "SYP";
        UpdateCurrencyButtons();
    }

    private void UsdButton_Click(object sender, RoutedEventArgs e)
    {
        _accountCurrency = "USD";
        UpdateCurrencyButtons();
    }

    private void UpdateCurrencyButtons()
    {
        Brush primary = Application.Current.Resources["PrimaryBrush"] as Brush ?? Brushes.Blue;
        Brush surfaceAlt = Application.Current.Resources["SurfaceAltBrush"] as Brush ?? Brushes.LightGray;
        Brush mutedText = Application.Current.Resources["MutedTextBrush"] as Brush ?? Brushes.Gray;

        bool isSyp = _accountCurrency == "SYP";

        SypButton.Background = isSyp ? primary : surfaceAlt;
        SypButton.Foreground = isSyp ? Brushes.White : mutedText;
        UsdButton.Background = isSyp ? surfaceAlt : primary;
        UsdButton.Foreground = isSyp ? mutedText : Brushes.White;
    }

    private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
    {
        string newName = NewCategoryTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(newName)) return;

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
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (TryReadForm()) DialogResult = true;
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
                MessageBoxButton.OK, MessageBoxImage.Warning);
            AccountNameTextBox.Focus();
            return false;
        }

        if (AccountGroupComboBox.SelectedItem is not AccountGroupOption selectedGroup)
        {
            MessageBox.Show(
                AppUiResources.GetString("InvalidAccountGroupMessage"),
                AppUiResources.GetString("InvalidDataTitle"),
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (!NumberFormatting.TryParseDecimal(InitialBalanceTextBox.Text, out decimal initialBalance) || initialBalance < 0)
        {
            MessageBox.Show(
                AppUiResources.GetString("InvalidInitialBalanceMessage"),
                AppUiResources.GetString("InvalidDataTitle"),
                MessageBoxButton.OK, MessageBoxImage.Warning);
            InitialBalanceTextBox.Focus();
            return false;
        }

        AccountName = accountName;
        AccountGroup = selectedGroup.Key;
        AccountCurrency = _accountCurrency;
        InitialBalance = initialBalance;
        return true;
    }
}
