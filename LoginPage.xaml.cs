using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Botana.Data;

namespace Botana;

public partial class LoginPage : ContentPage
{
    private const string LoggedKey = "user:logged";
    private const string EmailKey = "user:email";
    private const string IsAdminKey = "user:isAdmin";

    // жёстко заданный админ
    private const string AdminEmail = "tat@company.com";
    private const string AdminPass = "111111";

    private bool _isBusy;

    public LoginPage() => InitializeComponent();

    private async void OnSignInClicked(object sender, EventArgs e)
    {
        if (_isBusy) return;
        _isBusy = true;

        try
        {
            var email = (EmailEntry?.Text ?? string.Empty).Trim().ToLowerInvariant();
            var pwd = PasswordEntry?.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pwd))
            {
                await DisplayAlert("Sign in", "Please enter email and password.", "OK");
                return;
            }

            // 1) пробуем БД
            var (ok, isAdmin) = await UserDatabase.Instance.ValidateAsync(email, pwd);

            // 2) безусловный «хардкод» админа — поверх всего
            if (email.Equals(AdminEmail, StringComparison.OrdinalIgnoreCase) && pwd == AdminPass)
            {
                ok = true;
                isAdmin = true;
            }

            if (!ok)
            {
                var exists = await UserDatabase.Instance.FindByEmailAsync(email) != null;
                await DisplayAlert("Sign in", exists ? "Wrong password." : "Account not found.", "OK");

                if (exists)
                {
                    PasswordEntry.Text = string.Empty;
                    PasswordEntry.Focus();
                }
                return;
            }

            // 3) сохраняем сессию
            Preferences.Default.Set(LoggedKey, true);
            Preferences.Default.Set(EmailKey, email);
            Preferences.Default.Set(IsAdminKey, isAdmin);

            PasswordEntry.Text = string.Empty;

            // 4) переход
            await Shell.Current.GoToAsync(nameof(AccountPage));
        }
        catch (Exception ex)
        {
#if DEBUG
            await DisplayAlert("Sign in (debug)", ex.ToString(), "OK");
#else
            await DisplayAlert("Sign in", "Sign-in is temporarily unavailable. Please try again.", "OK");
#endif
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async void OnGoRegisterTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync(nameof(RegisterPage));

    private async void OnCloseClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");
}