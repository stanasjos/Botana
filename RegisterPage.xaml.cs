using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;   // Browser.OpenAsync
using Botana.Data;                       // UserDatabase

namespace Botana;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
    }

    private async void OnSignUpClicked(object sender, EventArgs e)
    {
        var fullName = NameEntry?.Text?.Trim() ?? string.Empty;
        var emailRaw = EmailEntry?.Text ?? string.Empty;
        var email = emailRaw.Trim().ToLowerInvariant();  // нормализуем
        var password = PasswordEntry?.Text ?? string.Empty;
        var confirm = ConfirmEntry?.Text ?? string.Empty;
        var accepted = TermsCheck?.IsChecked == true;

        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(confirm) ||
            !accepted)
        {
            await DisplayAlert("Register", "Fill out all fields and accept the terms.", "OK");
            return;
        }

        if (password != confirm)
        {
            await DisplayAlert("Register", "Passwords do not match.", "OK");
            return;
        }

        try
        {
            // Пытаемся создать пользователя; вернёт false, если такой email уже есть
            var created = await UserDatabase.Instance.CreateAsync(email, password, isAdmin: false);
            if (!created)
            {
                await DisplayAlert("Register", "This e-mail is already registered.", "OK");
                return;
            }

            // Успех: сообщаем и возвращаемся на экран входа
            await DisplayAlert("Register", "Account created. You can sign in now.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Register", $"Unexpected error:\n{ex.Message}", "OK");
        }
    }

    private async void OnGoLoginTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync(nameof(LoginPage));

    private async void OnCloseClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");

    // Tap на текст "I agree to Terms & Privacy"
    private async void OnTermsTapped(object? sender, TappedEventArgs e)
    {
        if (TermsCheck != null)
            TermsCheck.IsChecked = !TermsCheck.IsChecked;

        await Browser.OpenAsync("https://example.com/terms", BrowserLaunchMode.SystemPreferred);
    }
}