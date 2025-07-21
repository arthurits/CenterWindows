using CenterWindow.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CenterWindow.ViewModels;
public partial class ListWindowsViewModel : ObservableRecipient
{
    [ObservableProperty]
    public partial string StrCenterMenu { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StrCenterWithAlphaMenu { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StrTransparencyMenu { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StrDeselectMenu { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StrCenterAllMenu { get; set; } = string.Empty;

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        StrCenterMenu = "StrCenterMenu".GetLocalized("ListWindows");
        StrCenterWithAlphaMenu = "StrCenterWithAlphaMenu".GetLocalized("ListWindows");
        StrTransparencyMenu = "StrTransparencyMenu".GetLocalized("ListWindows");
        StrDeselectMenu = "StrDeselectMenu".GetLocalized("ListWindows");
        StrCenterAllMenu = "StrCenterAllMenu".GetLocalized("ListWindows");
    }
}
