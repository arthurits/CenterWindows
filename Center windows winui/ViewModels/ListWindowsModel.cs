using System.Collections.ObjectModel;
using CenterWindow.Contracts.Services;
using CenterWindow.Models;
using CenterWindow.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CenterWindow.ViewModels;

public partial class ListWindowsViewModel : ObservableRecipient
{
    // Services
    private readonly IWindowEnumerationService _enumerationService;
    private readonly IWindowCenterService _centerService;
    private readonly IMouseHookService _mouseHook;
    private readonly ITrayIconService _trayIconService;

    // Properties
    [ObservableProperty]
    public partial ObservableCollection<WindowModel> WindowsList { get; set; } = [];

    [ObservableProperty]
    public partial WindowModel? SelectedWindow { get; set; } = null;

    [ObservableProperty]
    public partial int Transparency { get; set; } = 255;

    public ListWindowsViewModel(
        IWindowEnumerationService enumerationService,
        IWindowCenterService centerService,
        IMouseHookService mouseHook,
        ITrayIconService trayIcon)
    {
        _enumerationService = enumerationService;
        _centerService = centerService;
        _mouseHook = mouseHook;
        _trayIconService = trayIcon;
        _trayIconService.TrayMenuItemClicked += OnTrayMenuItem;
        _trayIconService.TrayMenuOpening     += OnTrayMenuOpening;

        LoadWindows();
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

    [RelayCommand]
    private async Task SelectWindowAsync()
    {
        try
        {
            // Set the mouse hook to capture the window under the cursor
            var hWnd = await _mouseHook.CaptureWindowUnderCursorAsync();
            if (hWnd != IntPtr.Zero)
            {
                // If the window is already in the list, select it; otherwise, center it
                var win = WindowsList.FirstOrDefault(w => w.Hwnd == hWnd);
                if (win is not null)
                {
                    SelectedWindow = win;
                }
                else
                {
                    _centerService.CenterWindow(hWnd, 255);
                }
            }
        }
        catch (TaskCanceledException)
        {
        }
    }

    private void OnTrayMenuOpening(object? sender, TrayMenuOpeningEventArgs e)
    {
        int id = 1;

        // Open
        e.Items.Add(new TrayMenuItemDefinition
        {
            Id   = id++,
            Text = "Abrir"
        });

        // Windows list submenu
        var ventanas = new TrayMenuItemDefinition
        {
            Id   = id++,
            Text = "Ventanas"
        };
        foreach (var win in WindowsList)
        {
            ventanas.Children.Add(new TrayMenuItemDefinition
            {
                Id   = id++,
                Text = win.Title
            });
        }
        e.Items.Add(ventanas);

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
}
