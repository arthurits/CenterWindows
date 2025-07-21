using CommunityToolkit.Mvvm.ComponentModel;

namespace CenterWindow.ViewModels;
public partial class ListWindowsViewModel : ObservableRecipient
{
    [ObservableProperty]
    public partial string StrCenterMenu { get; set; } = "Centrar";

    [ObservableProperty]
    public partial string StrCenterWithAlphaMenu { get; set; } = "Centrar con alpha";

    [ObservableProperty]
    public partial string StrTransparencyMenu { get; set; } = "Transparencia";

    [ObservableProperty]
    public partial string StrDeselectMenu { get; set; } = "Deseleccionar";

    [ObservableProperty]
    public partial string StrCenterAllMenu { get; set; } = "Centrar todo";

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
    }
}
