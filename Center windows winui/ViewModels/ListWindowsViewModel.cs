using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CenterWindow.Contracts.Services;
using CenterWindow.Models;
using CenterWindow.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;

namespace CenterWindow.ViewModels;

public partial class ListWindowsViewModel : ObservableRecipient
{
    // Services
    private readonly IWindowEnumerationService _enumerationService;
    private readonly IWindowCenterService _centerService;
    private readonly IMouseHookService _mouseHook;
    private readonly ILocalizationService _localizationService;
    private readonly ITrayIconService _trayIconService;

    // Properties
    [ObservableProperty]
    public partial ObservableCollection<WindowModel> WindowsList { get; set; } = [];
    
    [ObservableProperty]
    public partial ObservableCollection<WindowModel> SelectedWindows { get; private set; } = [];

    //[ObservableProperty]
    //public partial WindowModel? SelectedWindow { get; set; } = null;

    public bool IsListItemSelected => SelectedWindows.Count > 0;

    [ObservableProperty]
    public partial int Transparency { get; set; } = 255;

    private byte _alpha => (byte)Math.Clamp(Transparency, 0, 255);

    [ObservableProperty]
    public partial bool IsAlphaChecked { get; set; } = false;
    [ObservableProperty]
    public partial bool IsCenterChecked { get; set; } = false;

    public ListWindowsViewModel(
        IWindowEnumerationService enumerationService,
        IWindowCenterService centerService,
        IMouseHookService mouseHook,
        ILocalizationService localizationService,
        ITrayIconService trayIcon)
    {
        _enumerationService = enumerationService;
        _centerService = centerService;
        _mouseHook = mouseHook;
        _localizationService = localizationService;
        _localizationService.LanguageChanged += OnLanguageChanged;
        _trayIconService = trayIcon;
        _trayIconService.TrayMenuItemClicked += OnTrayMenuItem;
        _trayIconService.TrayMenuOpening     += OnTrayMenuOpening;

        LoadWindows();

        // Load string resources into binding variables for the UI
        OnLanguageChanged(null, EventArgs.Empty);

        SelectedWindows.CollectionChanged += SelectedWindows_CollectionChanged;
    }

    [RelayCommand]
    public void LoadWindows()
    {
        var list = new ObservableCollection<WindowModel>();
        foreach (var window in _enumerationService.GetDesktopWindows())
        {
            list.Add(window);
        }

        WindowsList = list;
    }

    

    [RelayCommand]
    public void RefreshWindows()
    {
        LoadWindows();
    }

    [RelayCommand]
    private void ApplyToSelectedWindows()
    {
        foreach (var selectedWindow in SelectedWindows)
        {
            if (IsCenterChecked)
            {
                _centerService.CenterWindow(selectedWindow.Hwnd);
            }
            if (IsAlphaChecked)
            {
                _centerService.SetWindowTransparency(selectedWindow.Hwnd, _alpha);
            }
        }
    }

    [RelayCommand]
    private void ApplyToAllWindows()
    {
        foreach (var window in WindowsList)
        {
            if (IsCenterChecked)
            {
                _centerService.CenterWindow(window.Hwnd);
            }
            if (IsAlphaChecked)
            {
                _centerService.SetWindowTransparency(window.Hwnd, _alpha);
            }
        }
    }

    [RelayCommand]
    private void DeselectWindow()
    {
        if (SelectedWindows.Count > 0)
        {
            SelectedWindows.Clear();
        }
    }

    [RelayCommand]
    private void WindowsSelectionChanged(IList<object> selectedItems)
    {
        SelectedWindows.Clear();
        if (selectedItems is null)
        {
            return;
        }

        foreach (var item in selectedItems.OfType<WindowModel>())
        {
            SelectedWindows.Add(item);
        }
    }

    private void SelectedWindows_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(IsListItemSelected));
    }

    private void OnTrayMenuOpening(object? sender, TrayMenuOpeningEventArgs e)
    {
        int id = 1;

        // Open
        e.Items.Add(new TrayMenuItemDefinition
        {
            Id   = id++,
            Text = "Abrir",
            IsEnabled = false,
            IconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico")
        });

        // Windows list submenu
        var windows = new TrayMenuItemDefinition
        {
            Id   = id++,
            Text = "Ventanas",
            IconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Settings_windows.svg")
        };
        foreach (var window in WindowsList)
        {
            windows.Children.Add(new TrayMenuItemDefinition
            {
                Id   = id++,
                Text = window.Title
            });
        }
        e.Items.Add(windows);

        // Horizontal separator
        e.Items.Add(new TrayMenuItemDefinition
        {
            IsSeparator = true
        });

        // Add "Exit" option
        e.Items.Add(new TrayMenuItemDefinition
        {
            Id   = id++,
            Text = "Salir"
        });
    }

    private void OnTrayMenuItem(object? s, TrayMenuItemEventArgs e)
    {
        switch (e.ItemId)
        {
            //case TrayMenuItems.Open:
            //    OpenWindowCommand.Execute(null);
            //    break;
            //case TrayMenuItems.Settings:
            //    NavigateToSettingsCommand.Execute(null);
            //    break;
            //case TrayMenuItems.Exit:
            //    ExitCommand.Execute(null);
            //    break;
        }
    }
}
