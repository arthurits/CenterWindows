using System.Collections.ObjectModel;
using CenterWindow.Contracts.Services;
using CenterWindow.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CenterWindow.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    // Services
    private readonly IWindowEnumerationService _enumerationService;
    private readonly IWindowCenterService _centerService;
    private readonly IMouseHookService _mouseHook;

    // Properties
    [ObservableProperty]
    public partial ObservableCollection<WindowModel> Windows { get; set; }

    [ObservableProperty]
    public partial WindowModel SelectedWindow { get; set; }

    [ObservableProperty]
    public partial int Transparency { get; set; } = 255;

    public MainViewModel(IWindowEnumerationService enumerationService, IWindowCenterService centerService, IMouseHookService mouseHook)
    {
        _enumerationService = enumerationService;
        _centerService = centerService;
        _mouseHook = mouseHook;

        LoadWindows();
    }

    [RelayCommand]
    public void LoadWindows()
    {
        var list = new ObservableCollection<WindowModel>();
        foreach (var window in _enumerationService.GetDesktopWindows())
        {
            list.Add(window);
        }

        Windows = list;
    }

    [RelayCommand]
    public void CenterSelectedWindow()
    {
        if (SelectedWindow is not null)
        {
            var alpha = (byte)Math.Clamp(Transparency, 0, 255);
            _centerService.CenterWindow(SelectedWindow.Hwnd, alpha);
        }
    }

    [RelayCommand]
    public void RefreshWindows()
    {
        LoadWindows();
    }
}
