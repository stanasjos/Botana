using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel; // Launcher
using Botana.Data;

namespace Botana;

public partial class ExplorePage : ContentPage
{
    // Публичные элементы для привязки в XAML (ItemsSource)
    public ObservableCollection<Automation> PublicItems { get; } = new();

    // Команда для кнопки "Use template" в карточке
    public ICommand OpenTemplateCommand { get; }

    private Border? _selected;
    private string? _selectedCategory = "all";

    public ExplorePage()
    {
        InitializeComponent();

        // Команда открытия шаблона
        OpenTemplateCommand = new Command<Automation>(async a => await OpenTemplateAsync(a));

        // Привязываемся к себе (в XAML: ItemsSource="{Binding PublicItems}", Command="{Binding OpenTemplateCommand}")
        BindingContext = this;

        // По умолчанию выбираем "All" после загрузки визуального дерева
        Loaded += async (_, __) =>
        {
            Select(CatAll);
            await LoadPublicAsync();
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Актуализируем список при возврате на страницу
        await LoadPublicAsync();
    }

    // Поиск
    private async void OnSearchClicked(object sender, EventArgs e)
        => await LoadPublicAsync();

    // Тап по категории
    private async void OnCategoryTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border card)
        {
            Select(card);
            await LoadPublicAsync();
        }
    }

    // Выделение карточки категории (зелёная обводка, без заливки)
    private void Select(Border card)
    {
        if (_selected != null && _selected != card)
            SetCardState(_selected, false);

        SetCardState(card, true);
        _selected = card;
        _selectedCategory = card.AutomationId ?? "all";
    }

    private void SetCardState(Border card, bool isSelected)
    {
        var isDark = Application.Current!.UserAppTheme == AppTheme.Dark;

        var outlineNormal = (Color)Application.Current.Resources[
            isDark ? "DarkCardStroke" : "LightCardStroke"];
        var textNormal = (Color)Application.Current.Resources[
            isDark ? "DarkTextPrimary" : "LightTextPrimary"];
        var primary = (Color)Application.Current.Resources["Primary"];

        card.BackgroundColor = Colors.Transparent; // без заливки

        if (isSelected)
        {
            card.Stroke = new SolidColorBrush(primary);
            SetInnerLabelColor(card, primary);
        }
        else
        {
            card.Stroke = new SolidColorBrush(outlineNormal);
            SetInnerLabelColor(card, textNormal);
        }
    }

    private static void SetInnerLabelColor(Border card, Color color)
    {
        if (card.Content is Label lbl)
            lbl.TextColor = color;
        else if (card.Content is Layout layout)
            foreach (var l in layout.Children.OfType<Label>())
                l.TextColor = color;
    }

    // Загрузка/перезагрузка списка с учётом поиска и выбранной категории
    private async Task LoadPublicAsync()
    {
        var query = SearchEntry.Text?.Trim();
        var category = _selectedCategory;

        // если категория "all" — не фильтруем по категории
        if (string.Equals(category, "all", StringComparison.OrdinalIgnoreCase))
            category = null;

        // ВАЖНО: требуется метод в БД: GetPublicFilteredAsync(query, category, take)
        // Если его ещё нет — временно можно заменить на GetPublicAsync() и фильтровать в памяти.
        var rows = await AutomationDatabase.Instance
            .GetPublicFilteredAsync(query, category, take: 100);

        PublicItems.Clear();
        foreach (var a in rows)
            PublicItems.Add(a);
    }

    // Открыть ссылку шаблона и записать запуск как успешный
    private async Task OpenTemplateAsync(Automation? a)
    {
        if (a is null)
            return;

        if (string.IsNullOrWhiteSpace(a.SourceUrl))
        {
            await DisplayAlert("Open template", "No URL set for this template.", "OK");
            return;
        }

        // считаем как успешный запуск
        await AutomationDatabase.Instance.IncrementRunsAsync(a.Id, success: true);

        try
        {
            await Launcher.OpenAsync(a.SourceUrl);
        }
        catch
        {
            await DisplayAlert("Open template", "Cannot open URL.", "OK");
        }
    }
}