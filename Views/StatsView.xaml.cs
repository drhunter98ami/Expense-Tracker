using ExpenseTracker.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ExpenseTracker.Views;

public partial class StatsView
{
    private readonly StatsViewModel _viewModel;

    public StatsView()
    {
        InitializeComponent();
        _viewModel = new StatsViewModel();
        DataContext = _viewModel;
        
        // Subscribe to property changes to update chart visibility
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(StatsViewModel.SelectedTab))
            {
                UpdateChartVisibility();
            }
        };
        
        // Initial visibility update
        UpdateChartVisibility();
    }

    private void UpdateChartVisibility()
    {
        if (IncomeChart != null && ExpenseChart != null)
        {
            if (_viewModel.IsIncomeTab)
            {
                IncomeChart.Visibility = Visibility.Visible;
                ExpenseChart.Visibility = Visibility.Collapsed;
            }
            else
            {
                IncomeChart.Visibility = Visibility.Collapsed;
                ExpenseChart.Visibility = Visibility.Visible;
            }
        }
    }

    public void Refresh()
    {
        _viewModel.Refresh();
    }
}
