using CenterWindow.Contracts.Services;
using CenterWindow.Helpers;
using CenterWindow.Models;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;

namespace CenterWindow;

public sealed partial class MainWindow : WindowEx
{
    private readonly Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;

    private readonly UISettings settings;

    // Services
    private readonly ITrayIconService _trayMenuService;
    private readonly IMainWindowService _mainWindowService;

    public MainWindow()
    {
        InitializeComponent();

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/AppIcon.ico"));
        Content = null;
        //Title = "AppDisplayName".GetLocalized();

        // Theme change code picked from https://github.com/microsoft/WinUI-Gallery/pull/1239
        dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        settings = new UISettings();
        settings.ColorValuesChanged += Settings_ColorValuesChanged; // cannot use FrameworkElement.ActualThemeChanged event

        // Get services
        _trayMenuService   = App.GetService<ITrayIconService>();
        _mainWindowService = App.GetService<IMainWindowService>();
        _trayMenuService.TrayMenuItemClicked += OnTrayMenuItemClicked;
    }

    // this handles updating the caption button colors correctly when indows system theme is changed
    // while the app is open
    private void Settings_ColorValuesChanged(UISettings sender, object args)
    {
        // This calls comes off-thread, hence we will need to dispatch it to current app's thread
        dispatcherQueue.TryEnqueue(TitleBarHelper.ApplySystemThemeToCaptionButtons);
    }

    private void OnTrayMenuItemClicked(object? sender, TrayMenuItemEventArgs e)
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
}
