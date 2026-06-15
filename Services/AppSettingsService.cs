using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services;

public static class AppSettingsService
{
    private const int SettingsId = 1;

    public static AppSetting GetOrCreate()
    {
        using AppDbContext dbContext = new();

        AppSetting? settings = dbContext.AppSettings.Find(SettingsId);

        if (settings is not null)
            return settings;

        settings = new AppSetting
        {
            Id = SettingsId,
            UsdToSypRate = 15000,
            CurrencyCode = "SYP"
        };

        dbContext.AppSettings.Add(settings);
        dbContext.SaveChanges();
        return settings;
    }

    public static void SaveUsdToSypRate(decimal rate)
    {
        using AppDbContext dbContext = new();

        int rows = dbContext.Database.ExecuteSqlRaw(
            "UPDATE AppSettings SET UsdToSypRate = {0} WHERE Id = {1}",
            rate, SettingsId);

        if (rows == 0)
        {
            dbContext.Database.ExecuteSqlRaw(
                "INSERT INTO AppSettings (Id, UsdToSypRate, CurrencyCode) VALUES ({0}, {1}, 'SYP')",
                SettingsId, rate);
        }
    }

    public static void SaveCurrencyCode(string code)
    {
        using AppDbContext dbContext = new();

        int rows = dbContext.Database.ExecuteSqlRaw(
            "UPDATE AppSettings SET CurrencyCode = {0} WHERE Id = {1}",
            code, SettingsId);

        if (rows == 0)
        {
            dbContext.Database.ExecuteSqlRaw(
                "INSERT INTO AppSettings (Id, UsdToSypRate, CurrencyCode) VALUES ({0}, 15000, {1})",
                SettingsId, code);
        }
    }
}
