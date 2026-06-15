using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Botana;

public partial class AdminDashboardViewModel : ObservableObject
{
    [ObservableProperty] private int statsUsers;
    [ObservableProperty] private int statsVendors;
    [ObservableProperty] private int statsAdmins;
    [ObservableProperty] private int statsPending;

    public ObservableCollection<AutomationItem> Items { get; } = new();

    public AdminDashboardViewModel()
    {
        StatsUsers = 120; StatsVendors = 8; StatsAdmins = 2; StatsPending = 4;

        Items.Add(new AutomationItem
        {
            Id = 1,
            Title = "Google Sheets — Monthly Report",
            Description = "Собирает метрики и формирует отчёт.",
            Platform = "Make",
            Status = "На модерации",
            Seller = "Виктор"
        });

        Items.Add(new AutomationItem
        {
            Id = 2,
            Title = "Shop → Slack Alerts",
            Description = "Мгновенные уведомления о заказах.",
            Platform = "n8n",
            Status = "Активен",
            Seller = "Ирина"
        });
    }

    [RelayCommand]
    private async Task OpenDetails(int id)
        => await Shell.Current.DisplayAlert("Детали", $"Открыть карточку #{id}", "OK");

    [RelayCommand]
    private void Hide(int id)
    {
        var item = Items.FirstOrDefault(x => x.Id == id);
        if (item != null) item.Status = "Скрыт";
    }

    [RelayCommand]
    private async Task OpenSite(int id)
        => await Shell.Current.DisplayAlert("На сайте", $"Перейти к карточке #{id}", "OK");
}