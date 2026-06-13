using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace ExpenseTracker.Services;

public static class AppUiResources
{
    private static FlowDirection currentFlowDirection = FlowDirection.RightToLeft;
    private static XmlLanguage currentXmlLanguage = XmlLanguage.GetLanguage("ar-SA");

    public static void Apply(bool isEnglish, bool isDarkMode)
    {
        ApplyLanguage(isEnglish);
        ApplyTheme(isDarkMode);
        ApplyModeLabels(isEnglish, isDarkMode);
        ApplyToOpenWindows();
    }

    public static string GetString(string key)
    {
        return Application.Current.Resources[key] as string ?? key;
    }

    public static void ApplyToWindow(Window window)
    {
        window.FlowDirection = currentFlowDirection;
        window.Language = currentXmlLanguage;
    }

    private static void ApplyLanguage(bool isEnglish)
    {
        ResourceDictionary resources = Application.Current.Resources;

        currentFlowDirection = isEnglish ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
        currentXmlLanguage = XmlLanguage.GetLanguage(isEnglish ? "en-US" : "ar-SA");
        resources["AppTitle"] = isEnglish ? "Expense Tracker" : "متتبع المصروفات";
        resources["AppSubtitle"] = isEnglish ? "Money at a glance" : "نظرة واضحة على أموالك";
        resources["TransactionsNavText"] = isEnglish ? "Transactions" : "المعاملات";
        resources["StatsNavText"] = isEnglish ? "Stats" : "الإحصائيات";
        resources["AccountsNavText"] = isEnglish ? "Accounts" : "الحسابات";
        resources["SettingsNavText"] = isEnglish ? "Settings" : "الإعدادات";
        resources["LanguageLabel"] = isEnglish ? "Language" : "اللغة";
        resources["LanguageOptionText"] = isEnglish ? "English" : "العربية";
        resources["ThemeLabel"] = isEnglish ? "Theme" : "المظهر";
        resources["TransactionsTitle"] = isEnglish ? "Transactions" : "المعاملات";
        resources["TransactionsSubtitle"] = isEnglish
            ? "Review spending and income by day"
            : "راجع المصروفات والدخل حسب اليوم";
        resources["AddTransactionText"] = isEnglish ? "+ Add Transaction" : "+ إضافة معاملة";
        resources["NoDescriptionText"] = isEnglish ? "No description" : "بدون وصف";
        resources["UncategorizedText"] = isEnglish ? "Uncategorized" : "غير مصنف";
        resources["NoAccountText"] = isEnglish ? "No account" : "بدون حساب";
        resources["AddTransactionDialogTitle"] = isEnglish ? "Add Transaction" : "إضافة معاملة";
        resources["AmountLabel"] = isEnglish ? "Amount" : "المبلغ";
        resources["DateLabel"] = isEnglish ? "Date" : "التاريخ";
        resources["AccountLabel"] = isEnglish ? "Account" : "الحساب";
        resources["CategoryLabel"] = isEnglish ? "Category" : "التصنيف";
        resources["DescriptionLabel"] = isEnglish ? "Description" : "الوصف";
        resources["SaveText"] = isEnglish ? "Save" : "حفظ";
        resources["CancelText"] = isEnglish ? "Cancel" : "إلغاء";
        resources["StatsPlaceholderText"] = isEnglish ? "Stats" : "الإحصائيات";
        resources["AccountsPlaceholderText"] = isEnglish ? "Accounts" : "الحسابات";
        resources["AccountsTitle"] = isEnglish ? "Accounts" : "الحسابات";
        resources["AccountsSubtitle"] = isEnglish
            ? "Track account balances and total net worth"
            : "تابع أرصدة الحسابات وصافي الرصيد";
        resources["TotalNetWorthLabel"] = isEnglish ? "Total Net Worth" : "الرصيد الإجمالي";
        resources["CurrentBalanceLabel"] = isEnglish ? "Current balance" : "الرصيد الحالي";
        resources["AddAccountText"] = isEnglish ? "+ Add Account" : "+ إضافة حساب";
        resources["AddAccountDialogTitle"] = isEnglish ? "Add Account" : "إضافة حساب";
        resources["AccountNameLabel"] = isEnglish ? "Account name" : "اسم الحساب";
        resources["InitialBalanceLabel"] = isEnglish ? "Opening balance" : "الرصيد الافتتاحي";
        resources["InitialBalanceCategoryHint"] = isEnglish
            ? "Choose an income category when the opening balance is greater than zero."
            : "اختر تصنيف دخل عند إدخال رصيد افتتاحي أكبر من صفر.";
        resources["InitialBalanceDescription"] = isEnglish ? "Opening balance" : "رصيد افتتاحي";
        resources["InvalidInitialBalanceMessage"] = isEnglish
            ? "Enter a valid opening balance of zero or more."
            : "أدخل رصيداً افتتاحياً صحيحاً أكبر من أو يساوي صفر.";
        resources["MissingIncomeCategoryMessage"] = isEnglish
            ? "Add an income category before setting an opening balance."
            : "أضف تصنيف دخل قبل تعيين رصيد افتتاحي.";
        resources["MissingLookupTitle"] = isEnglish ? "Cannot add transaction" : "لا يمكن إضافة معاملة";
        resources["MissingLookupMessage"] = isEnglish
            ? "Add an account and a category before creating a transaction."
            : "أضف حساباً وتصنيفاً أولاً قبل إنشاء معاملة.";
        resources["InvalidDataTitle"] = isEnglish ? "Missing information" : "بيانات غير مكتملة";
        resources["InvalidAmountMessage"] = isEnglish
            ? "Enter a valid amount greater than zero."
            : "أدخل مبلغاً صحيحاً أكبر من صفر.";
        resources["MissingDateMessage"] = isEnglish ? "Choose the transaction date." : "اختر تاريخ المعاملة.";
        resources["MissingAccountMessage"] = isEnglish ? "Choose the account." : "اختر الحساب.";
        resources["MissingCategoryMessage"] = isEnglish ? "Choose the category." : "اختر التصنيف.";
        resources["InvalidAccountNameMessage"] = isEnglish
            ? "Enter an account name."
            : "أدخل اسم الحساب.";
        resources["SettingsTitle"] = isEnglish ? "Settings" : "الإعدادات";
        resources["SettingsSubtitle"] = isEnglish
            ? "Manage categories and the USD to SYP exchange rate"
            : "أدر التصنيفات وسعر صرف الدولار مقابل الليرة السورية";
        resources["IncomeCategoriesTitle"] = isEnglish ? "Income categories" : "تصنيفات الدخل";
        resources["ExpenseCategoriesTitle"] = isEnglish ? "Expense categories" : "تصنيفات المصروف";
        resources["AddCategoryText"] = isEnglish ? "Add" : "إضافة";
        resources["ExchangeRateTitle"] = isEnglish ? "Exchange rate" : "سعر الصرف";
        resources["ExchangeRateSubtitle"] = isEnglish
            ? "Set the value of 1 US dollar in Syrian pounds"
            : "حدد قيمة 1 دولار أمريكي بالليرة السورية";
        resources["UsdToSypRateLabel"] = isEnglish ? "1 USD = SYP" : "1 USD = SYP";
        resources["InvalidCategoryNameMessage"] = isEnglish
            ? "Enter a category name."
            : "أدخل اسم التصنيف.";
        resources["DuplicateCategoryMessage"] = isEnglish
            ? "This category already exists."
            : "هذا التصنيف موجود بالفعل.";
        resources["InvalidExchangeRateMessage"] = isEnglish
            ? "Enter a valid exchange rate greater than zero."
            : "أدخل سعر صرف صحيحاً أكبر من صفر.";
    }

    private static void ApplyModeLabels(bool isEnglish, bool isDarkMode)
    {
        Application.Current.Resources["ThemeOptionText"] = (isEnglish, isDarkMode) switch
        {
            (true, true) => "Dark",
            (true, false) => "Light",
            (false, true) => "داكن",
            _ => "فاتح"
        };
    }

    private static void ApplyTheme(bool isDarkMode)
    {
        if (isDarkMode)
        {
            SetTheme(
                "#0F172A",
                "#111827",
                "#1F2937",
                "#334155",
                "#F8FAFC",
                "#CBD5E1",
                "#38BDF8",
                "#0EA5E9",
                "#172554",
                "#1E3A8A",
                "#0F766E");
            return;
        }

        SetTheme(
            "#F8FAFC",
            "#FFFFFF",
            "#F1F5F9",
            "#DDE5F0",
            "#0F172A",
            "#64748B",
            "#2563EB",
            "#1D4ED8",
            "#DBEAFE",
            "#E0F2FE",
            "#0F766E");
    }

    private static void SetTheme(
        string appBackground,
        string surface,
        string surfaceAlt,
        string border,
        string text,
        string mutedText,
        string primary,
        string primaryHover,
        string active,
        string header,
        string success)
    {
        ResourceDictionary resources = Application.Current.Resources;

        resources["AppBackgroundBrush"] = CreateBrush(appBackground);
        resources["SurfaceBrush"] = CreateBrush(surface);
        resources["SurfaceAltBrush"] = CreateBrush(surfaceAlt);
        resources["BorderBrush"] = CreateBrush(border);
        resources["TextBrush"] = CreateBrush(text);
        resources["MutedTextBrush"] = CreateBrush(mutedText);
        resources["PrimaryBrush"] = CreateBrush(primary);
        resources["PrimaryHoverBrush"] = CreateBrush(primaryHover);
        resources["ActiveBrush"] = CreateBrush(active);
        resources["GroupHeaderBrush"] = CreateBrush(header);
        resources["SuccessBrush"] = CreateBrush(success);
        resources["OnPrimaryBrush"] = CreateBrush("#FFFFFF");
        resources["ShadowBrush"] = CreateBrush("#0F172A");
    }

    private static void ApplyToOpenWindows()
    {
        foreach (Window window in Application.Current.Windows)
        {
            ApplyToWindow(window);
        }
    }

    private static SolidColorBrush CreateBrush(string color)
    {
        SolidColorBrush brush = new((Color)ColorConverter.ConvertFromString(color));
        brush.Freeze();
        return brush;
    }
}
