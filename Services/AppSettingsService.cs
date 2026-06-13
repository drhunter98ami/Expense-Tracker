using ExpenseTracker.Models;

namespace ExpenseTracker.Services;

public static class AppSettingsService
{
    private const int SettingsId = 1;

    public static AppSetting GetOrCreate()
    {
        using AppDbContext dbContext = new();

        AppSetting? settings = dbContext.AppSettings.Find(SettingsId);

        if (settings is not null)
        {
            return settings;
        }

        settings = new AppSetting
        {
            Id = SettingsId,
            UsdToSypRate = 15000
        };

        dbContext.AppSettings.Add(settings);
        dbContext.SaveChanges();

        return settings;
    }

    public static void SaveUsdToSypRate(decimal rate)
    {
        using AppDbContext dbContext = new();

        AppSetting? settings = dbContext.AppSettings.Find(SettingsId);

        if (settings is null)
        {
            settings = new AppSetting
            {
                Id = SettingsId,
                UsdToSypRate = rate
            };
            dbContext.AppSettings.Add(settings);
        }
        else
        {
            settings.UsdToSypRate = rate;
        }

        dbContext.SaveChanges();
    }
}
