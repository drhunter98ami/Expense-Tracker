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
    }

    public void Refresh()
    {
        _viewModel.Refresh();
    }
}
