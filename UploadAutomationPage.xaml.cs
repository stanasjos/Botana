// UploadAutomationPage.xaml.cs
using Botana.Data;                  // NEW
using Microsoft.Maui.Storage;       // NEW

namespace Botana;

public partial class UploadAutomationPage : ContentPage
{
    public UploadAutomationPage()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");

    // можно оставить как заглушку
    private async void OnChooseFileClicked(object sender, EventArgs e)
        => await DisplayAlert("Upload", "File picker will be here.", "OK");

    // NEW: создать запись в БД и вернуться к списку
    private async void OnUploadClicked(object sender, EventArgs e)
    {
        var email = (Preferences.Default.Get("user:email", string.Empty) ?? string.Empty)
                    .Trim().ToLowerInvariant();

        var name = NameEntry?.Text?.Trim();          // Entry с x:Name="NameEntry"
        var src = SourceUrlEntry?.Text?.Trim();     // Entry с x:Name="SourceUrlEntry" (необязательно)

        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlert("Upload", "Please enter a name.", "OK");
            return;
        }

        await AutomationDatabase.Instance.CreateAsync(email, name, src);
        await DisplayAlert("Upload", "Automation added.", "OK");

        // Вернуться на список — он обновится в OnAppearing
        await Shell.Current.GoToAsync(nameof(MyAutomations));
    }
}