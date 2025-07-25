﻿using System.Collections.ObjectModel;
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
    private readonly ILocalizationService _localizationService;
    private readonly ITrayIconService _trayIconService;

    // Properties
    [ObservableProperty]
    public partial ObservableCollection<WindowModel> WindowsList { get; set; } = [];

    [ObservableProperty]
    public partial WindowModel? SelectedWindow { get; set; } = null;

    public bool IsListItemSelected => SelectedWindow is not null;

    [ObservableProperty]
    public partial int Transparency { get; set; } = 255;

    private byte _alpha => (byte)Math.Clamp(Transparency, 0, 255);

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

    //[RelayCommand]
    //public void CenterSelectedWindow()
    //{
    //    if (SelectedWindow is not null)
    //    {
    //        _centerService.CenterWindow(SelectedWindow.Hwnd, _alpha);
    //    }
    //}

    [RelayCommand]
    public void RefreshWindows()
    {
        LoadWindows();
    }

    //[RelayCommand]
    //private async Task SelectWindowAsync()
    //{
    //    try
    //    {
    //        //// Set the mouse hook to capture the window under the cursor
    //        //var hWnd = await _mouseHook.CaptureWindowUnderCursorAsync();
    //        //if (hWnd != IntPtr.Zero)
    //        //{
    //        //    // If the window is already in the list, select it; otherwise, center it
    //        //    var win = WindowsList.FirstOrDefault(w => w.Hwnd == hWnd);
    //        //    if (win is not null)
    //        //    {
    //        //        SelectedWindow = win;
    //        //    }
    //        //    else
    //        //    {
    //        //        _centerService.CenterWindow(hWnd, 255);
    //        //    }
    //        //}
    //    }
    //    catch (TaskCanceledException)
    //    {
    //    }
    //}

    [RelayCommand]
    private void CenterMenu(WindowModel window)
    {
        _centerService.CenterWindow(window.Hwnd);
    }

    [RelayCommand]
    private void CenterWithAlphaMenu(WindowModel window)
    {
        _centerService.CenterWindow(window.Hwnd);
        _centerService.SetWindowTransparency(window.Hwnd, _alpha);
    }

    [RelayCommand]
    private void TransparencyMenu(WindowModel window)
    {
        _centerService.SetWindowTransparency(window.Hwnd, _alpha);
    }

    [RelayCommand]
    private void DeselectWindowMenu()
    {
        SelectedWindow = null;
    }
    
    // 5) Centrar todas las ventanas de la lista
    [RelayCommand]
    private void CenterAllMenu()
    {
        foreach (var w in WindowsList)
        {
            _centerService.CenterWindow(w.Hwnd);
        }
    }

    partial void OnSelectedWindowChanged(WindowModel? value)
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
