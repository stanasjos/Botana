using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;          // NEW
using System.Windows.Input;
using Microsoft.Maui.Controls;         // NEW
using Microsoft.Maui.Storage;
using Botana.Data;

namespace Botana;

public partial class MyAutomations : ContentPage
{
    private readonly ObservableCollection<Automation> _items = new();

    public ICommand TogglePauseCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand OpenSettingsCommand { get; }   // NEW

    public MyAutomations()
    {
        InitializeComponent();

        AutomationsView.ItemsSource = _items;

        TogglePauseCommand = new Command<int>(async id => await TogglePauseAsync(id));
        DeleteCommand = new Command<int>(async id => await DeleteAsync(id));
        OpenSettingsCommand = new Command<int>(async id => await OpenSettingsAsync(id)); // NEW

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _items.Clear();

        var email = (Preferences.Default.Get("user:email", string.Empty) ?? string.Empty)
                    .Trim().ToLowerInvariant();

        var rows = await AutomationDatabase.Instance.GetByOwnerAsync(email);

        int active = 0, totalRuns = 0, success = 0;
        foreach (var a in rows)
        {
            _items.Add(a);
            if (a.IsActive) active++;
            totalRuns += a.TotalRuns;
            success += a.SuccessRuns;
        }

        KpiActiveLabel.Text = active.ToString();
        KpiTotalLabel.Text = totalRuns.ToString();
        KpiRateLabel.Text = totalRuns > 0 ? $"{(int)Math.Round(success * 100.0 / totalRuns)}%" : "0%";
    }

    private async Task TogglePauseAsync(int id)
    {
        await AutomationDatabase.Instance.TogglePauseAsync(id);
        await LoadAsync();
    }

    private async Task DeleteAsync(int id)
    {
        var ok = await DisplayAlert("Delete", "Delete this automation?", "Delete", "Cancel");
        if (!ok) return;

        await AutomationDatabase.Instance.DeleteAsync(id);
        await LoadAsync();
    }

    // NEW: переход на страницу настроек конкретной автоматизации
    private Task OpenSettingsAsync(int id)
        => Shell.Current.GoToAsync($"{nameof(AutomationSettingsPage)}?id={id}");

    private async void OnBackClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");

    private async void OnUploadClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync(nameof(UploadAutomationPage));
}