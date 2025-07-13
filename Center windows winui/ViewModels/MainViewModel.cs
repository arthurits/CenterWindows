using System.Collections.ObjectModel;
using CenterWindow.Contracts.Services;
using CenterWindow.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CenterWindow.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    // Services
    readonly IWindowEnumerationService _enumerationService;
    readonly IWindowCenterService _centerService;

    // Properties
    [ObservableProperty]
    public partial ObservableCollection<WindowModel> Windows { get; set; }

    [ObservableProperty]
    public partial WindowModel SelectedWindow { get; set; }

    [ObservableProperty]
    public partial byte Transparency { get; set; } = 255;

    public MainViewModel(IWindowEnumerationService enumerationService, IWindowCenterService centerService)
    {
        _enumerationService = enumerationService;
        _centerService = centerService;

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
            _centerService.CenterWindow(SelectedWindow.Hwnd, Transparency);
        }
    }

    [RelayCommand]
    public void RefreshWindows()
    {
        LoadWindows();
    }
}
