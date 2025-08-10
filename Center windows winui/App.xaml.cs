using System.Runtime.InteropServices;
using CenterWindow.Activation;
using CenterWindow.Contracts.Services;
using CenterWindow.Helpers;
using CenterWindow.Models;
using CenterWindow.Services;
using CenterWindow.ViewModels;
using CenterWindow.Views;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace CenterWindow;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host { get; }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static UIElement? AppTitlebar { get; set; }

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers

            // Services
            services.AddTransient<INavigationViewService, NavigationViewService>();
            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<ILocalizationService, LocalizationService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddSingleton<ILocalSettingsService<AppSettings>, LocalSettingsService>();
            services.AddSingleton<IFileService, FileService>();

            // CenterWindow services
            services.AddSingleton<IWindowCenterService, WindowCenterService>();
            services.AddSingleton<IWindowEnumerationService, WindowEnumerationService>();

            // Mouse Hook Service
            services.AddSingleton<IMouseHookService, MouseHookService>();

            // Window Highlight Service
            services.AddSingleton<IWindowHighlightService, WindowHighlightService>();

            // Tray Icon Service
            services.AddSingleton<GdiPlusIconLoader>();
            services.AddSingleton<Win2DIconLoader>();
            services.AddSingleton<IconLoaderFactory>();
            services.AddSingleton<ITrayIconService, TrayIconService>(sp => new TrayIconService(MainWindow, sp.GetRequiredService<IconLoaderFactory>()));

            // Register the MainWindow service. We use the factory method instantiation of MainWindow but
            // leave the option to use the inline if needed in the future.
            services.AddSingleton<WindowEx>(sp => MainWindow);
            services.AddSingleton<IMainWindowService, MainWindowService>();
            //services.AddSingleton<IMainWindowService>(sp => new MainWindowService(MainWindow));

            // Startup service
            services.AddSingleton<IStartupService, StartupService>();

            // Views and ViewModels
            services.AddSingleton<SelectWindowViewModel>();
            services.AddTransient<SelectWindowPage>();
            services.AddSingleton<AboutViewModel>();
            services.AddTransient<AboutPage>();
            services.AddSingleton<SettingsViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddSingleton<ListWindowsViewModel>();
            services.AddTransient<ListWindowsPage>();
            services.AddTransient<ShellPage>();
            services.AddSingleton<ShellViewModel>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.

        //// Muestra el detalle de la excepción
        //System.Diagnostics.Debug.WriteLine($"ERROR UI: {e.Exception}");

        //// Impide que vuelva a romper en el XAML-generated hook
        //e.Handled = true;
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        //await App.GetService<IActivationService>().ActivateAsync(args);

        // Handle closing event
        MainWindow.AppWindow.Closing += OnClosing;
        MainWindow.Closed += OnClosed;

        // Read settings and set initial window position
        // https://github.com/arthurits/OpenXML-editor/blob/master/OpenXML%20WinUI/App.xaml.cs
        var settings = App.GetService<ILocalSettingsService<AppSettings>>();
        await settings.ReadSettingFileAsync<AppSettings>();

        if (settings.GetValues.WindowPosition)
        {
            MainWindow.MoveAndResize(settings.GetValues.WindowLeft, settings.GetValues.WindowTop, settings.GetValues.WindowWidth, settings.GetValues.WindowHeight);
        }
        else
        {
            // https://stackoverflow.com/questions/74890047/how-can-i-set-my-winui3-program-to-be-started-in-the-center-of-the-screen
            //IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(m_window);
            //WindowPosition.SetWindowSize(MainWindow, width: 1000, height: 900);
            WindowPosition.CenterWindow(MainWindow);
        }

        // Now that the settings are loaded, we call the activation service
        // so that the defaul page and view model can access the settings and apply them
        await App.GetService<IActivationService>().ActivateAsync(args);
        
        // Apply theme stored in settings
        var themeService = App.GetService<IThemeSelectorService>();
        if (themeService is not null)
        {
            if (Enum.TryParse(settings.GetValues.ThemeName, out ElementTheme theme) is true)
            {
                await themeService.SetThemeAsync(theme);
            }
        }

        // Apply language stored in settings
        var _localizationService = App.GetService<ILocalizationService>();
        _localizationService.SetAppLanguage(settings.GetValues.AppCultureName);

        // Initialize the tray icon service
        var _trayIconService = App.GetService<ITrayIconService>();
        if (settings.GetValues.ShowTrayIcon)
        {
            _trayIconService.Initialize();
        }

        // If the app is set to launch at startup, hide the main window and initialize the tray icon
        var mainWindowService = App.GetService<IMainWindowService>();
        if (settings.GetValues.LaunchAtStartup)
        {
            mainWindowService.Hide();
            _trayIconService.Initialize();  // This should be already initializated, but we ensure the tray icon is set up
            settings.GetValues.LaunchAtStartup = true; // Ensure the setting is true
        }

        //// Just in case we mess with the system cursors, reset them
        //uint SPI_SETCURSORS = 0x0057;
        //uint SPIF_SENDCHANGE = 0x02;
        //SystemParametersInfo(SPI_SETCURSORS, 0, IntPtr.Zero, SPIF_SENDCHANGE);

        //[DllImport("user32.dll", SetLastError = true)]
        //static extern bool SystemParametersInfo(
        //uint uiAction,
        //uint uiParam,
        //IntPtr pvParam,
        //uint fWinIni);
    }

    private async void OnClosing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        args.Cancel = true; // https://github.com/microsoft/WindowsAppSDK/issues/3209

        if (await ConfirmAppCloseAsync())
        {
            MainWindow.Close();
            args.Cancel = false; // Allow the app to close
        }
    }

    public static async Task<bool> ConfirmAppCloseAsync()
    {

        var result = await MessageBox.Show(
            "MsgBoxExitContent".GetLocalized("MessageBox"),
            "MsgBoxExitTitle".GetLocalized("MessageBox"),
            primaryButtonText: "MsgBoxExitPrimary".GetLocalized("MessageBox"),
            closeButtonText: "MsgBoxExitCancel".GetLocalized("MessageBox"),
            defaultButton: MessageBox.MessageBoxButtonDefault.CloseButton,
            icon: MessageBox.MessageBoxImage.Question);

        var ClosingConfirmed = result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary;

        if (ClosingConfirmed)
        {
            var settings = App.GetService<ILocalSettingsService<AppSettings>>();
            //await settings.SaveSettingKeyAsync<string>("isTrue","yes");
            if (settings.GetValues.WindowPosition)
            {
                settings.GetValues.WindowLeft = MainWindow.AppWindow.Position.X;
                settings.GetValues.WindowTop = MainWindow.AppWindow.Position.Y;
                settings.GetValues.WindowWidth = MainWindow.AppWindow.Size.Width;
                settings.GetValues.WindowHeight = MainWindow.AppWindow.Size.Height;
            }

            settings.GetValues.AppCultureName = App.GetService<ILocalizationService>().CurrentLanguage;

            // No need to save the theme here, as it is already set in SettingsViewModel OnThemeChanged
            //var themeService = App.GetService<IThemeSelectorService>();
            //settings.GetValues.ThemeName = themeService.GetThemeName();

            await settings.SaveSettingFileAsync();

            // Set the startup enabled state based on the settings
            var startupService = App.GetService<IStartupService>();
            startupService.SetStartupEnabled(settings.GetValues.LaunchAtStartup);
        }

        return ClosingConfirmed;
    }

    private async void OnClosed(object sender, WindowEventArgs args)
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(
            DispatcherQueuePriority.Low,
            new DispatcherQueueHandler(async () =>
            {
                await Host.StopAsync();
                Host.Dispose();
            }));
    }
}
