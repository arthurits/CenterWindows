using CenterWindow.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CenterWindow.ViewModels;
public partial class ListWindowsViewModel : ObservableRecipient, IDisposable
{
    [ObservableProperty]
    public partial string StrCenter { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StrCenterToolTip { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StrTransparency { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StrTransparencyToolTip { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StrApplyToSelected { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StrApplyToSelectedToolTip { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StrApplyToAll { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StrApplyToAllToolTip { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StrDeselect { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StrDeselectToolTip { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StrRefresh { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StrRefreshToolTip { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StrColumnHeaderTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StrColumnHeaderHandle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StrColumnHeaderProcess { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StrColumnHeaderClass { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StrColumnHeaderDimensions { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StrTransparencyText))]
    public partial string StrTransparencyHeader { get; set; } = string.Empty;

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        // Flyout menu and CommandBar buttons
        StrCenter = "StrCenter".GetLocalized("ListWindows");
        StrCenterToolTip = "StrCenterToolTip".GetLocalized("ListWindows");
        StrTransparency = "StrTransparency".GetLocalized("ListWindows");
        StrTransparencyToolTip = "StrTransparencyToolTip".GetLocalized("ListWindows");
        StrApplyToSelected = "StrApplyToSelected".GetLocalized("ListWindows");
        StrApplyToSelectedToolTip = "StrApplyToSelectedToolTip".GetLocalized("ListWindows");
        StrApplyToAll = "StrApplyToAll".GetLocalized("ListWindows");
        StrApplyToAllToolTip = "StrApplyToAllToolTip".GetLocalized("ListWindows");
        StrDeselect = "StrDeselect".GetLocalized("ListWindows");
        StrDeselectToolTip = "StrDeselectToolTip".GetLocalized("ListWindows");
        StrRefresh = "StrRefresh".GetLocalized("ListWindows");
        StrRefreshToolTip = "StrRefreshToolTip".GetLocalized("ListWindows");

        // Column headers
        StrColumnHeaderTitle = "StrColumnHeaderTitle".GetLocalized("ListWindows");
        StrColumnHeaderHandle = "StrColumnHeaderHandle".GetLocalized("ListWindows");
        StrColumnHeaderProcess = "StrColumnHeaderProcess".GetLocalized("ListWindows");
        StrColumnHeaderClass = "StrColumnHeaderClass".GetLocalized("ListWindows");
        StrColumnHeaderDimensions = "StrColumnHeaderDimensions".GetLocalized("ListWindows");

        // Header for the transparency slider
        StrTransparencyHeader = "StrTransparencyHeader".GetLocalized("ListWindows");
    }
}
