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

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        // Header for the crosshair drag mode
        StrDragCrossHair = "StrDragCrossHair".GetLocalized("SelectWindow");

        // Header for the transparency slider
        StrTransparencyHeader = "StrTransparencyHeader".GetLocalized("SelectWindow");
    }
}
