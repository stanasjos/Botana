using Botana.Data;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace Botana;

[QueryProperty(nameof(AutoId), "id")]
public partial class AutomationSettingsPage : ContentPage
{
    public int AutoId { get; set; }
    private Automation? _item;

    public AutomationSettingsPage() => InitializeComponent();

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _item = await AutomationDatabase.Instance.GetByIdAsync(AutoId);
        if (_item == null)
        {
            await DisplayAlert("Error", "Not found.", "OK");
            await Shell.Current.GoToAsync("..");
            return;
        }

        NameEntry.Text = _item.Name;
        UrlEntry.Text = _item.SourceUrl;
        DescEditor.Text = _item.Description;
        PublicSwitch.IsToggled = _item.IsPublic;

        // NEW: категория
        CategoryPicker.SelectedItem = string.IsNullOrWhiteSpace(_item.Category)
            ? "general"
            : _item.Category;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_item == null) return;

        _item.Name = (NameEntry?.Text ?? string.Empty).Trim();

        var url = UrlEntry?.Text?.Trim();
        _item.SourceUrl = string.IsNullOrWhiteSpace(url) ? null : url;

        var desc = DescEditor?.Text?.Trim();
        _item.Description = string.IsNullOrWhiteSpace(desc) ? null : desc;

        _item.IsPublic = PublicSwitch?.IsToggled == true;

        // NEW: сохранить категорию
        _item.Category = (CategoryPicker?.SelectedItem as string) ?? "general";

        await AutomationDatabase.Instance.UpdateAsync(_item);
        await DisplayAlert("Saved", "Automation updated.", "OK");
        await Shell.Current.GoToAsync("..");
    }

    private async void OnOpenClicked(object sender, EventArgs e)
    {
        var url = UrlEntry?.Text?.Trim();
        if (string.IsNullOrWhiteSpace(url))
        {
            await DisplayAlert("Open", "No URL set.", "OK");
            return;
        }

        var opened = true;
        try { await Launcher.OpenAsync(url); }
        catch { opened = false; await DisplayAlert("Open", "Cannot open URL.", "OK"); }

        await AutomationDatabase.Instance.IncrementRunsAsync(AutoId, success: opened);
    }

    private async void OnBackClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");
}