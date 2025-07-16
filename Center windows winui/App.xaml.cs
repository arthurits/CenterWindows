using System.Runtime.InteropServices;
using CenterWindow.Activation;
using CenterWindow.Contracts.Services;
using CenterWindow.Interop;
using CenterWindow.Models;
using CenterWindow.Services;
using CenterWindow.Settings;
using CenterWindow.ViewModels;
using CenterWindow.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
    public IHost Host
    {
        get;
    }

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

    public static UIElement? AppTitlebar
    {
        get; set;
    }

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

            // Views and ViewModels
            services.AddSingleton<AboutViewModel>();
            services.AddTransient<AboutPage>();
            services.AddSingleton<SettingsViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddSingleton<MainViewModel>();
            services.AddTransient<MainPage>();
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

        await App.GetService<IActivationService>().ActivateAsync(args);

        // Handle closing event
        MainWindow.AppWindow.Closing += OnClosing;

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
            //WindowPosition.CenterWindow(MainWindow);
        }

        // Apply theme stored in settings
        var themeService = App.GetService<IThemeSelectorService>();
        if (themeService is not null)
        {
            if (Enum.TryParse(settings.GetValues.ThemeName, out ElementTheme theme) is true)
            {
                //themeService.SetTheme(theme);
            }
        }

        // Apply language stored in settings
        var _localizationService = App.GetService<ILocalizationService>();
        _localizationService.SetAppLanguage(settings.GetValues.AppCultureName);

        // Just in case we mess the system cursors, reset them
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
        //args.Cancel = true; // https://github.com/microsoft/WindowsAppSDK/issues/3209

        //var result = await MessageBox.Show(
        //    "MsgBoxExitContent".GetLocalized("MessageBox"),
        //    "MsgBoxExitTitle".GetLocalized("MessageBox"),
        //    primaryButtonText: "MsgBoxExitPrimary".GetLocalized("MessageBox"),
        //    closeButtonText: "MsgBoxExitCancel".GetLocalized("MessageBox"),
        //    defaultButton: MessageBox.MessageBoxButtonDefault.CloseButton,
        //    icon: MessageBox.MessageBoxImage.Question);

        //if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        //{
        //    var settings = App.GetService<ILocalSettingsService<AppSettings>>();
        //    //await settings.SaveSettingKeyAsync<string>("isTrue","yes");
        //    if (settings.GetValues.WindowPosition)
        //    {
        //        settings.GetValues.WindowLeft = MainWindow.AppWindow.Position.X;
        //        settings.GetValues.WindowTop = MainWindow.AppWindow.Position.Y;
        //        settings.GetValues.WindowWidth = MainWindow.AppWindow.Size.Width;
        //        settings.GetValues.WindowHeight = MainWindow.AppWindow.Size.Height;
        //    }

        //    settings.GetValues.AppCultureName = App.GetService<ILocalizationService>().CurrentLanguage;
        //    //var themeService = App.GetService<IThemeSelectorService>();
        //    //settings.GetValues.ThemeName = themeService.GetThemeName();

        //    await settings.SaveSettingFileAsync();

        //    MainWindow.Close();
        //}
    }
}
