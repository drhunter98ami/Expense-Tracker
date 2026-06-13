using System.Globalization;
using System.Windows.Data;
using ExpenseTracker.Services;

namespace ExpenseTracker.Converters;

public class EnglishNumberConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string format = parameter as string ?? "N2";

        return value switch
        {
            decimal decimalValue => NumberFormatting.Format(decimalValue, format),
            double doubleValue => NumberFormatting.Format(doubleValue, format),
            float floatValue => NumberFormatting.Format((decimal)floatValue, format),
            int intValue => NumberFormatting.Format(intValue, format),
            _ => value?.ToString() ?? string.Empty
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
