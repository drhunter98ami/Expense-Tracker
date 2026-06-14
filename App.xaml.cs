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

        EnsureDatabase();

        base.OnStartup(e);
    }

    private static void EnsureDatabase()
    {
        using AppDbContext dbContext = new();

        dbContext.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS AppSettings (
                Id INTEGER NOT NULL CONSTRAINT PK_AppSettings PRIMARY KEY,
                UsdToSypRate TEXT NOT NULL DEFAULT '15000'
            )
            """);

        dbContext.Database.ExecuteSqlRaw("""
            INSERT OR IGNORE INTO AppSettings (Id, UsdToSypRate) VALUES (1, '15000')
            """);

        try
        {
            dbContext.Database.ExecuteSqlRaw("""
                ALTER TABLE Accounts ADD COLUMN "Group" TEXT NOT NULL DEFAULT 'Cash'
                """);
        }
        catch { }

        try
        {
            dbContext.Database.ExecuteSqlRaw("""
                ALTER TABLE AppSettings ADD COLUMN CurrencyCode TEXT NOT NULL DEFAULT 'SYP'
                """);
        }
        catch { }
    }
}
