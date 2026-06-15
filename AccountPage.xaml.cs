using Microsoft.Maui.Storage;

namespace Botana;

public partial class AccountPage : ContentPage
{
    private const string LoggedKey = "user:logged";
    private const string EmailKey = "user:email";
    private const string IsAdminKey = "user:isAdmin";

    // ↓↓↓ ДОБАВЛЕНО: хардкод e-mail админа (как в LoginPage)
    private const string AdminEmail = "tat@company.com";
    public AccountPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // ?????????? e-mail ? ?????? ? ???????
        var email = Preferences.Default.Get(EmailKey, string.Empty);
        EmailLabel.Text = string.IsNullOrWhiteSpace(email) ? "unknown" : email;

        var isAdmin = Preferences.Default.Get(IsAdminKey, false);
        AdminButton.IsVisible = isAdmin;
    }

    // ????????? ?? ???????? ????????
    private async void OnSettingsClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync(nameof(SettingsPage));

    private async void OnMyAutomationsClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync(nameof(MyAutomations));

    private async void OnUploadClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync(nameof(UploadAutomationPage));

    private async void OnOpenAdminClicked(object sender, EventArgs e)
    {
        var isAdmin = Preferences.Default.Get(IsAdminKey, false);
        if (!isAdmin)
        {
            await DisplayAlert("Access denied", "Admin rights required.", "OK");
            return;
        }
        await Shell.Current.GoToAsync(nameof(AdminPage));
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        Preferences.Default.Remove(LoggedKey);
        Preferences.Default.Remove(EmailKey);
        Preferences.Default.Remove(IsAdminKey);

        await DisplayAlert("Logged out", "You have been signed out.", "OK");
        await Shell.Current.GoToAsync(".."); // ????????? ????? ????? ??????
    }
}