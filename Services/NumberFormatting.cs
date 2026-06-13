using System.Globalization;
using System.Text;

namespace ExpenseTracker.Services;

public static class NumberFormatting
{
    public static CultureInfo EnglishCulture { get; } = CultureInfo.GetCultureInfo("en-US");

    public static string Format(decimal value, string format = "N2")
    {
        return value.ToString(format, EnglishCulture);
    }

    public static string Format(double value, string format = "N2")
    {
        return value.ToString(format, EnglishCulture);
    }

    public static bool TryParseDecimal(string? value, out decimal result)
    {
        result = 0;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string normalized = NormalizeDigits(value.Trim());

        return decimal.TryParse(
            normalized,
            NumberStyles.Number,
            EnglishCulture,
            out result);
    }

    public static string NormalizeDigits(string value)
    {
        StringBuilder builder = new(value.Length);

        foreach (char character in value)
        {
            builder.Append(character switch
            {
                '٠' => '0',
                '١' => '1',
                '٢' => '2',
                '٣' => '3',
                '٤' => '4',
                '٥' => '5',
                '٦' => '6',
                '٧' => '7',
                '٨' => '8',
                '٩' => '9',
                '۰' => '0',
                '۱' => '1',
                '۲' => '2',
                '۳' => '3',
                '۴' => '4',
                '۵' => '5',
                '۶' => '6',
                '۷' => '7',
                '۸' => '8',
                '۹' => '9',
                '٫' => '.',
                '،' => ',',
                _ => character
            });
        }

        return builder.ToString();
    }

    public static CultureInfo CreateArabicUiCulture()
    {
        CultureInfo culture = new("ar-SA");
        culture.NumberFormat.NativeDigits = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"];
        culture.NumberFormat.DigitSubstitution = DigitShapes.None;
        return culture;
    }
}
