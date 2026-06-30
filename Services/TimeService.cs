using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ExpenseTracker.Services;

public static class TimeService
{
    private const int HourOffset = 7;

    public static DateTime Now => DateTime.Now.AddHours(HourOffset);
    
    public static DateTime Today => DateTime.Today.AddHours(HourOffset);
    
    public static DateTime UtcNow => DateTime.UtcNow.AddHours(HourOffset);
}

public class ClockViewModel : INotifyPropertyChanged
{
    private string _currentTime = string.Empty;
    private string _currentDate = string.Empty;
    
    public string CurrentTime
    {
        get => _currentTime;
        private set
        {
            if (_currentTime != value)
            {
                _currentTime = value;
                OnPropertyChanged();
            }
        }
    }

    public string CurrentDate
    {
        get => _currentDate;
        private set
        {
            if (_currentDate != value)
            {
                _currentDate = value;
                OnPropertyChanged();
            }
        }
    }

    public ClockViewModel()
    {
        UpdateTime();
        StartClock();
    }

    private void UpdateTime()
    {
        DateTime adjustedTime = TimeService.Now;
        CurrentTime = adjustedTime.ToString("hh:mm:ss tt");
        CurrentDate = adjustedTime.ToString("dddd, dd/MM/yyyy");
    }

    private void StartClock()
    {
        System.Windows.Threading.DispatcherTimer timer = new()
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        
        timer.Tick += (s, e) => UpdateTime();
        timer.Start();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
