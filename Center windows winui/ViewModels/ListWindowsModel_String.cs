using CommunityToolkit.Mvvm.ComponentModel;

namespace CenterWindow.ViewModels;
public partial class ListWindowsViewModel : ObservableRecipient
{
    [ObservableProperty]
    private partial string StrCenterMenuText { get; set; } = "Centrar";

    [ObservableProperty]
    private partial string StrCenterWithAlphaMenuText { get; set; } = "Centrar con alpha";

    [ObservableProperty]
    private partial string StrTransparencyMenuText { get; set; } = "Transparencia";

    [ObservableProperty]
    private partial string StrDeselectMenuText { get; set; } = "Deseleccionar";

    [ObservableProperty]
    private partial string StrCenterAllMenuText { get; set; } = "Centrar todo";
}
