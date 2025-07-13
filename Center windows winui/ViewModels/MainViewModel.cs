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

    public MainViewModel(IWindowEnumerationService enumerationService, IWindowCenterService centerService)
    {
        _enumerationService = enumerationService;
        _centerService = centerService;

        LoadWindows();
    }

    [RelayCommand]
    private void LoadWindows()
    {
        var list = new ObservableCollection<WindowModel>();
        foreach (var window in _enumerationService.GetDesktopWindows())
        {
            list.Add(window);
        }

        Windows = list;
    }
}
