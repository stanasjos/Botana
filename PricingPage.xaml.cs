using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Botana;

public partial class PricingPage : ContentPage
{
    public PricingPage()
    {
        InitializeComponent();
        Reset(FreeCard);
        Reset(ProCard);
        Reset(TeamCard);
    }

    private void OnCardTapped(object sender, EventArgs e)
        => Select((sender as Button)?.Parent?.Parent?.Parent as Border ?? sender as Border);

    private void Select(Border? card)
    {
        if (card is null) return;

        Reset(FreeCard);
        Reset(ProCard);
        Reset(TeamCard);

        card.Stroke = new SolidColorBrush((Color)Application.Current!.Resources["Primary"]);
        card.Shadow = new Shadow
        {
            Brush = (Brush)Application.Current!.Resources["GlowBrush"],
            Opacity = 0.60f,
            Radius = 28f,
            Offset = new Point(0, 10)
        };

        card.ScaleTo(1.02, 120);
    }

    private static void Reset(Border? card)
    {
        if (card is null) return;

        var strokeKey = Application.Current!.UserAppTheme == AppTheme.Dark
            ? "DarkCardStroke" : "LightCardStroke";

        card.StrokeThickness = 1;
        card.Stroke = new SolidColorBrush((Color)Application.Current!.Resources[strokeKey]);
        card.Shadow = new Shadow
        {
            Brush = (Brush)Application.Current!.Resources["GlowBrush"],
            Opacity = 0.55f,
            Radius = 24f,
            Offset = new Point(0, 8)
        };

        card.ScaleTo(1.0, 80);
    }
}