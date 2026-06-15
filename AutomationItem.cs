using CommunityToolkit.Mvvm.ComponentModel;

namespace Botana;

public partial class AutomationItem : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty] private string title = "";
    [ObservableProperty] private string description = "";
    [ObservableProperty] private string platform = "";
    [ObservableProperty] private string status = "";
    [ObservableProperty] private string seller = "";
}