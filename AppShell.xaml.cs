using Microsoft.Maui.Storage;

namespace Botana;

public partial class AppShell : Shell
{
    private const string ThemeKey = "theme_dark";
    private const string LoggedKey = "user:logged";

    public AppShell()
    {
        InitializeComponent();

        // Маршруты
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(AccountPage), typeof(AccountPage));
        Routing.RegisterRoute(nameof(AdminPage), typeof(AdminPage));
        Routing.RegisterRoute(nameof(AutomationSettingsPage), typeof(AutomationSettingsPage));


        // НОВЫЕ СТРАНИЦЫ
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        Routing.RegisterRoute(nameof(MyAutomations), typeof(MyAutomations));
        Routing.RegisterRoute(nameof(UploadAutomationPage), typeof(UploadAutomationPage));
        // Применяем сохранённую тему и выставляем правильную иконку
        ApplyTheme(LoadSavedDark());
    }

    // Клик по иконке темы (луна/солнце)
    private async void OnThemeButtonClicked(object? sender, EventArgs e)
    {
        // лёгкая анимация
        if (ThemeButton != null)
        {
            await ThemeButton.ScaleTo(0.9, 90, Easing.CubicIn);
            await ThemeButton.ScaleTo(1.0, 90, Easing.CubicOut);
        }

        bool wantDark = Application.Current!.UserAppTheme != AppTheme.Dark;
        ApplyTheme(wantDark);
    }

    private void ApplyTheme(bool dark)
    {
        Application.Current!.UserAppTheme = dark ? AppTheme.Dark : AppTheme.Light;
        Preferences.Default.Set(ThemeKey, dark);

        // Меняем иконку: темная → луна, светлая → солнце
        if (ThemeButton != null)
            ThemeButton.Source = dark ? "dark_mode_2.png" : "light_mode.png";
    }

    private static bool LoadSavedDark()
    {
        if (Preferences.Default.ContainsKey(ThemeKey))
            return Preferences.Default.Get(ThemeKey, false);

        return Application.Current!.RequestedTheme == AppTheme.Dark;
    }

    // Профиль: если залогинен → AccountPage, иначе → LoginPage
    private async void OnProfileTapped(object? sender, EventArgs e)
    {
        var isLogged = Preferences.Default.Get(LoggedKey, false);
              await Shell.Current.GoToAsync(isLogged ? nameof(AccountPage) : nameof(LoginPage));
    }
}