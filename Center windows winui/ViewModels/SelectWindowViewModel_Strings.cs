using CenterWindow.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CenterWindow.ViewModels;
public partial class SelectWindowViewModel : ObservableRecipient, IDisposable
{
    [ObservableProperty]
    public partial string StrDragCrossHair { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StrTransparencyText))]
    public partial string StrTransparencyHeader { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StrWindowText { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrWindowHandle { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrWindowModuleName { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrWindowClassName { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrWindowLocation { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StrWindowDimensions { get; set; } = string.Empty;

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        // Header for the crosshair drag mode
        StrDragCrossHair = "StrDragCrossHair".GetLocalized("SelectWindow");

        // Header for the transparency slider
        StrTransparencyHeader = "StrTransparencyHeader".GetLocalized("SelectWindow");

        // Window output properties headers
        StrWindowText = "StrWindowText".GetLocalized("SelectWindow");
        StrWindowHandle = "StrWindowHandle".GetLocalized("SelectWindow");
        StrWindowModuleName = "StrWindowModuleName".GetLocalized("SelectWindow");
        StrWindowClassName = "StrWindowClassName".GetLocalized("SelectWindow");
        StrWindowLocation = "StrWindowLocation".GetLocalized("SelectWindow");
        StrWindowDimensions = "StrWindowDimensions".GetLocalized("SelectWindow");
    }
}
