using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel; // ← добавлено для Launcher.OpenAsync

namespace Botana;

public partial class WorkflowViewModel : ObservableObject
{
    // Список карточек, на который биндинги в XAML
    [ObservableProperty]
    private ObservableCollection<WorkflowModel> workflows = new();

    public WorkflowViewModel()
    {
        LoadWorkflows(); // простая синхронная инициализация без warning'ов
    }

    private void LoadWorkflows()
    {
        var seed = new List<WorkflowModel>
        {
            new("Google Sheets → Monthly Report", "Collects metrics and generates a report..")
            {
                // поставь сюда свою реальную ссылку, когда будет
                TargetUrl = "https://github.com/stanasjos"
            },
            new("Shop → Slack Alerts", "Instant order notifications.")
            {
                TargetUrl = "https://example.com/slack-alerts"
            },
            new("Forms → CRM Leads", "Automatically creates leads from form submissions.")
            {
                TargetUrl = "https://example.com/crm-leads"
            },
            new("Invoices → Bookkeeping", "Parses invoices and records them in Excel.")
            {
                TargetUrl = "https://example.com/bookkeeping"
            }
        };

        // Один раз присваиваем коллекцию — удобно для снапшота на странице
        Workflows = new ObservableCollection<WorkflowModel>(seed);
    }

    // Команда для кнопки "Use template" (XAML ссылается на неё)
    [RelayCommand]
    private async Task OpenTemplate(WorkflowModel? item)
    {
        if (item is null) return;

        // показываем твоё окно (одна кнопка "OK")
        await Shell.Current.DisplayAlert("Template", $"Open: {item.Title}", "OK");

        // после "OK" — открываем ссылку
        if (string.IsNullOrWhiteSpace(item.TargetUrl))
        {
            await Shell.Current.DisplayAlert("Open", "No URL configured.", "OK");
            return;
        }

        try
        {
            await Launcher.OpenAsync(item.TargetUrl);
        }
        catch
        {
            await Shell.Current.DisplayAlert("Open", "Cannot open URL.", "OK");
        }

        // Если захочешь подтверждение с двумя кнопками:
        // var confirm = await Shell.Current.DisplayAlert("Template", $"Open: {item.Title}?", "Open", "Cancel");
        // if (confirm) await Launcher.OpenAsync(item.TargetUrl);
    }
}

public class WorkflowModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // ← добавлено: куда переходить после нажатия OK в диалоге
    public string? TargetUrl { get; set; }

    public WorkflowModel() { }
    public WorkflowModel(string title, string description)
    {
        Title = title;
        Description = description;
    }
}