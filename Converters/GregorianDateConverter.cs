using System.Globalization;
using System.Windows.Data;

namespace ExpenseTracker.Converters;

public class GregorianDateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            // Use invariant culture to force Gregorian calendar
            CultureInfo gregorianCulture = CultureInfo.InvariantCulture;
            
            string format = parameter as string ?? "dddd, dd MMMM yyyy";
            return dateTime.ToString(format, gregorianCulture);
        }
        
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
