using CenterWindow.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CenterWindow.ViewModels;
public partial class SelectWindowViewModel : ObservableRecipient, IDisposable
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StrTransparencyText))]
    public partial string StrTransparencyHeader { get; set; } = string.Empty;

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        // Header for the transparency slider
        StrTransparencyHeader = "StrTransparencyHeader".GetLocalized("SelectWindow");
    }
}
