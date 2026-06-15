using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel; // Launcher

namespace Botana;

public partial class MainPage : ContentPage
{
    private readonly WorkflowViewModel _viewModel;
    private readonly List<WorkflowModel> _allWorkflows = new();

    // Команда для кнопки "Use template"
    public ICommand OpenTemplateCommand { get; }

    public MainPage()
    {
        InitializeComponent();

        _viewModel = new WorkflowViewModel();
        BindingContext = _viewModel;

        OpenTemplateCommand = new Command<WorkflowModel>(async w => await OpenTemplateAsync(w));
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Один раз кэшируем исходный список для локального поиска
        if (_allWorkflows.Count == 0 && _viewModel.Workflows?.Count > 0)
        {
            _allWorkflows.Clear();
            _allWorkflows.AddRange(_viewModel.Workflows);
        }
    }

    // Показываем диалог "Template", затем по OK открываем ссылку
    private async Task OpenTemplateAsync(WorkflowModel? w)
    {
        if (w is null) return;

        if (string.IsNullOrWhiteSpace(w.TargetUrl))
        {
            await DisplayAlert("Template", "Для этого шаблона не задана ссылка.", "OK");
            return;
        }

        // Твоё окно (OK-диалог)
        await DisplayAlert("Template", $"Open: {w.Title}", "OK");

        // После закрытия окна пробуем открыть ссылку
        try
        {
            await Launcher.OpenAsync(w.TargetUrl);
        }
        catch
        {
            await DisplayAlert("Template", "Не удалось открыть ссылку.", "OK");
        }
    }

    // Поиск
    private void ApplyFilter(string? query)
    {
        var q = (query ?? string.Empty).Trim();
        IEnumerable<WorkflowModel> src = _allWorkflows;

        if (!string.IsNullOrWhiteSpace(q))
        {
            src = src.Where(w =>
                (!string.IsNullOrEmpty(w.Title) && w.Title.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(w.Description) && w.Description.Contains(q, StringComparison.OrdinalIgnoreCase)));
        }

        _viewModel.Workflows.Clear();
        foreach (var item in src)
            _viewModel.Workflows.Add(item);
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        => ApplyFilter(e.NewTextValue);

    private void OnSearchClicked(object sender, EventArgs e)
        => ApplyFilter(SearchEntry?.Text);
}