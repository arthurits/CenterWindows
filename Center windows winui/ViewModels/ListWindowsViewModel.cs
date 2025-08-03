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

public partial class ListWindowsViewModel : ObservableRecipient, IDisposable
{
    // Services
    private readonly IWindowEnumerationService _enumerationService;
    private readonly IWindowCenterService _centerService;
    private readonly IMouseHookService _mouseHook;
    private readonly ILocalizationService _localizationService;
    private readonly ITrayIconService _trayIconService;
    private readonly IMainWindowService _mainWindowService;

    // Properties
    [ObservableProperty]
    public partial ObservableCollection<WindowModel> WindowsList { get; set; } = [];
    
    [ObservableProperty]
    public partial ObservableCollection<WindowModel> SelectedWindows { get; private set; } = [];

    //[ObservableProperty]
    //public partial WindowModel? SelectedWindow { get; set; } = null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StrTransparencyText))]
    public partial int Transparency { get; set; } = 255;

    public string StrTransparencyText => $"{StrTransparencyHeader}: {Alpha}";

    private byte Alpha => (byte)Math.Clamp(Transparency, 0, 255);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsApplyToAllEnabled))]
    [NotifyPropertyChangedFor(nameof(IsApplyToSelectedEnabled))]
    public partial bool IsAlphaChecked { get; set; } = false;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsApplyToAllEnabled))]
    [NotifyPropertyChangedFor(nameof(IsApplyToSelectedEnabled))]
    public partial bool IsCenterChecked { get; set; } = false;
    public bool IsApplyToAllEnabled => IsAlphaChecked || IsCenterChecked;
    public bool IsApplyToSelectedEnabled => (IsAlphaChecked || IsCenterChecked) && SelectedWindows.Count > 0;
    public bool IsDeselectEnabled => SelectedWindows.Count > 0;

    public ListWindowsViewModel(
        IWindowEnumerationService enumerationService,
        IWindowCenterService centerService,
        IMouseHookService mouseHook,
        ILocalizationService localizationService,
        ITrayIconService trayIcon,
        IMainWindowService mainWindowService)
    {
        _enumerationService = enumerationService;
        _centerService = centerService;
        _mouseHook = mouseHook;
        _localizationService = localizationService;
        _localizationService.LanguageChanged += OnLanguageChanged;
        _trayIconService = trayIcon;
        _trayIconService.TrayMenuItemClicked += OnTrayMenuItem;
        _trayIconService.TrayMenuOpening     += OnTrayMenuOpening;
        _trayIconService.TrayMenuIconDoubleClicked  += OnTrayMenuDoubleClicked;
        _mainWindowService = mainWindowService;

        RefreshWindows();

        SelectedWindows.CollectionChanged += SelectedWindows_CollectionChanged;
    }

    public void Dispose()
    {
        _localizationService.LanguageChanged -= OnLanguageChanged;
        _trayIconService.TrayMenuItemClicked -= OnTrayMenuItem;
        _trayIconService.TrayMenuOpening     -= OnTrayMenuOpening;
        _trayIconService.TrayMenuIconDoubleClicked -= OnTrayMenuDoubleClicked;
        SelectedWindows.CollectionChanged    -= SelectedWindows_CollectionChanged;
    }

    [RelayCommand]
    public void RefreshWindows()
    {
        var list = new ObservableCollection<WindowModel>();
        foreach (var window in _enumerationService.GetDesktopWindows())
        {
            list.Add(window);
        }

        WindowsList = list;
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
                _centerService.SetWindowTransparency(selectedWindow.Hwnd, Alpha);
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
                _centerService.SetWindowTransparency(window.Hwnd, Alpha);
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
        OnPropertyChanged(nameof(IsApplyToSelectedEnabled));
        OnPropertyChanged(nameof(IsDeselectEnabled));
    }

    private void OnTrayMenuOpening(object? sender, TrayMenuOpeningEventArgs e)
    {
        var id = (int)TrayMenuItemId.Exit;

        // Open
        e.Items.Add(new TrayMenuItemDefinition
        {
            Id   = (int)TrayMenuItemId.Open,
            Text = "Abrir",
            IsEnabled = false,
            IconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico")
        });

        // Windows list submenu
        RefreshWindows();
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
            Id   = (int)TrayMenuItemId.Exit,
            Text = "Salir"
        });
    }

    private void OnTrayMenuItem(object? s, TrayMenuItemEventArgs e)
    {
        switch (e.ItemId)
        {
            case (int)TrayMenuItemId.Open:
                _mainWindowService.Show();
                break;
            case (int)TrayMenuItemId.Exit:
                App.Current.Exit();
                break;
        }
    }

    private void OnTrayMenuDoubleClicked(object? sender, EventArgs e)
    {
        _mainWindowService.Show();
    }
}
