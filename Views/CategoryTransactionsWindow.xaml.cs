using System.Windows;
using ExpenseTracker.Models;
using ExpenseTracker.ViewModels;

namespace ExpenseTracker.Views;

public partial class CategoryTransactionsWindow : Window
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public CategoryType CategoryType { get; set; }
    public int Period { get; set; }
    public DateTime CurrentWeekStart { get; set; }
    public DateTime CurrentWeekEnd { get; set; }
    public int CurrentMonth { get; set; }
    public int CurrentYear { get; set; }
    public DateTime? PeriodStartDate { get; set; }
    public DateTime? PeriodEndDate { get; set; }

    public CategoryTransactionsWindow()
    {
        InitializeComponent();
        DataContext = new CategoryTransactionsViewModel();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        if (DataContext is CategoryTransactionsViewModel viewModel)
        {
            viewModel.CategoryId = CategoryId;
            viewModel.CategoryName = CategoryName;
            viewModel.CategoryType = CategoryType;
            viewModel.Period = Period;
            viewModel.CurrentWeekStart = CurrentWeekStart;
            viewModel.CurrentWeekEnd = CurrentWeekEnd;
            viewModel.CurrentMonth = CurrentMonth;
            viewModel.CurrentYear = CurrentYear;
            viewModel.PeriodStartDate = PeriodStartDate;
            viewModel.PeriodEndDate = PeriodEndDate;

            viewModel.LoadData();
        }
    }
}
