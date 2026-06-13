using System.Globalization;
using System.Threading;
using System.Windows;
using ExpenseTracker.Services;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        CultureInfo arabicCulture = NumberFormatting.CreateArabicUiCulture();
        Thread.CurrentThread.CurrentCulture = arabicCulture;
        Thread.CurrentThread.CurrentUICulture = arabicCulture;

        using AppDbContext dbContext = new();
        dbContext.Database.Migrate();

        base.OnStartup(e);
    }
}
