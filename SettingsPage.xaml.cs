using Microsoft.Maui.Storage;
using Botana.Data; // NEW

namespace Botana;

public partial class SettingsPage : ContentPage
{
    // profile keys (оставляем как кэш для UI)
    private const string FirstNameKey = "profile:first";
    private const string LastNameKey = "profile:last";
    private const string EmailKey = "profile:email";
    private const string CompanyKey = "profile:company";

    // notification prefs
    private const string EmailNotifKey = "pref:email_notif";
    private const string WorkflowAlertsKey = "pref:wf_alerts";
    private const string AutoUpdateKey = "pref:auto_update";

    public SettingsPage() => InitializeComponent();

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // заполнение из Preferences (как было)
        FirstNameEntry.Text = Preferences.Default.Get(FirstNameKey, string.Empty);
        LastNameEntry.Text = Preferences.Default.Get(LastNameKey, string.Empty);
        EmailEntry.Text = Preferences.Default.Get(EmailKey, string.Empty);
        CompanyEntry.Text = Preferences.Default.Get(CompanyKey, string.Empty);

        EmailNotifSwitch.IsToggled = Preferences.Default.Get(EmailNotifKey, true);
        WorkflowAlertsSwitch.IsToggled = Preferences.Default.Get(WorkflowAlertsKey, true);
        AutoUpdateSwitch.IsToggled = Preferences.Default.Get(AutoUpdateKey, false);

        // + подгружаем из SQLite по текущей сессии (если есть)
        var sessionEmail = Preferences.Default.Get("user:email", string.Empty);
        if (!string.IsNullOrWhiteSpace(sessionEmail))
        {
            var u = await UserDatabase.Instance.FindByEmailAsync(sessionEmail);
            if (u != null)
            {
                if (!string.IsNullOrWhiteSpace(u.FirstName)) FirstNameEntry.Text = u.FirstName;
                if (!string.IsNullOrWhiteSpace(u.LastName)) LastNameEntry.Text = u.LastName;
                if (!string.IsNullOrWhiteSpace(u.Company)) CompanyEntry.Text = u.Company;
                EmailEntry.Text = u.Email;
            }
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");

    private async void OnSaveProfileClicked(object sender, EventArgs e)
    {
        var first = FirstNameEntry.Text?.Trim() ?? "";
        var last = LastNameEntry.Text?.Trim() ?? "";
        var email = EmailEntry.Text?.Trim() ?? "";
        var company = CompanyEntry.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(first) ||
            string.IsNullOrWhiteSpace(last) ||
            string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlert("Missing data", "First name, last name and e-mail are required.", "OK");
            return;
        }

        // сохраняем быстрый кэш
        Preferences.Default.Set(FirstNameKey, first);
        Preferences.Default.Set(LastNameKey, last);
        Preferences.Default.Set(EmailKey, email);
        Preferences.Default.Set(CompanyKey, company);

        // синхронизируем с БД
        var current = Preferences.Default.Get("user:email", string.Empty);
        try
        {
            var (ok, newEmail) = await UserDatabase.Instance
                .UpdateProfileAsync(current, email, first, last, company);

            if (ok && !string.IsNullOrWhiteSpace(newEmail) &&
                !string.Equals(newEmail, current, StringComparison.OrdinalIgnoreCase))
            {
                Preferences.Default.Set("user:email", newEmail);
            }

            await DisplayAlert("Saved", "Profile has been updated.", "OK");
        }
        catch (InvalidOperationException)
        {
            await DisplayAlert("Error", "This e-mail is already in use.", "OK");
        }
        catch
        {
            await DisplayAlert("Error", "Could not update profile. Please try again.", "OK");
        }
    }

    private void OnEmailNotifToggled(object sender, ToggledEventArgs e)
        => Preferences.Default.Set(EmailNotifKey, e.Value);

    private void OnWorkflowAlertsToggled(object sender, ToggledEventArgs e)
        => Preferences.Default.Set(WorkflowAlertsKey, e.Value);

    private void OnAutoUpdateToggled(object sender, ToggledEventArgs e)
        => Preferences.Default.Set(AutoUpdateKey, e.Value);

    private async void OnUpdatePasswordClicked(object sender, EventArgs e)
    {
        var email = Preferences.Default.Get("user:email", string.Empty);
        var current = CurrentPasswordEntry.Text ?? "";
        var next = NewPasswordEntry.Text ?? "";
        var confirm = ConfirmPasswordEntry.Text ?? "";

        if (string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlert("Security", "You are not signed in.", "OK");
            return;
        }
        if (string.IsNullOrWhiteSpace(next) || next != confirm)
        {
            await DisplayAlert("Error", "Passwords don't match.", "OK");
            return;
        }

        var ok = await UserDatabase.Instance.ChangePasswordAsync(email, current, next);
        if (!ok)
        {
            await DisplayAlert("Security", "Current password is incorrect.", "OK");
            return;
        }

        CurrentPasswordEntry.Text = NewPasswordEntry.Text = ConfirmPasswordEntry.Text = string.Empty;
        await DisplayAlert("Security", "Password updated.", "OK");
    }

    private async void OnChangePlanClicked(object sender, EventArgs e)
    {
        try { await Shell.Current.GoToAsync(nameof(PricingPage)); }
        catch { await DisplayAlert("Billing", "Pricing/Billing is not implemented yet.", "OK"); }
    }
}