using CenterWindow.Contracts.Services;
using CenterWindow.Helpers;
using CenterWindow.Models;
using Microsoft.UI.Xaml;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;

namespace CenterWindow;

public sealed partial class MainWindow : WindowEx
{
    private readonly Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;

    private readonly UISettings settings;

    public MainWindow()
    {
        InitializeComponent();
        
        this.Activated += MainWindow_Activated;

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/AppIcon.ico"));
        Content = null;
        //Title = "AppDisplayName".GetLocalized();

        // Enable custom title bar handling at window level
        ExtendsContentIntoTitleBar = true;

        // Theme change code picked from https://github.com/microsoft/WinUI-Gallery/pull/1239
        dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        settings = new UISettings();
        settings.ColorValuesChanged += Settings_ColorValuesChanged; // cannot use FrameworkElement.ActualThemeChanged event

        AppWindow.Hide();
    }

    // this handles updating the caption button colors correctly when indows system theme is changed
    // while the app is open
    private void Settings_ColorValuesChanged(UISettings sender, object args)
    {
        // This calls comes off-thread, hence we will need to dispatch it to current app's thread
        dispatcherQueue.TryEnqueue(TitleBarHelper.ApplySystemThemeToCaptionButtons);
    }

    // New: allow pages to register the UIElement that will act as the title bar
    public void RegisterTitleBar(UIElement? titleBarElement)
    {
        // keep a single global reference for other helpers
        App.AppTitlebar = titleBarElement;

        if (titleBarElement is not null)
        {
            // Tell the window which element is the draggable title bar
            SetTitleBar(titleBarElement);
        }
        else
        {
            // Remove custom title bar if null
            SetTitleBar(null);
        }
    }

    private void MainWindow_Activated(object? sender, WindowActivatedEventArgs args)
    {
        // Asegurar que App.AppTitlebar apunta a la referencia registrada (si la hay)
        if (App.AppTitlebar == null)
        {
            // Intenta obtener la referencia tipada si ya se registró
            // Nada se hace si no hay titlebar registrado
            return;
        }

        // Forzar que App.AppTitlebar sea la referencia visual que registramos (no el TextBlock)
        // No hacemos cast directo; asumimos que RegisterTitleBar ya dejó la referencia correcta
        // Actualizar los colores de los botones según el tema actual
        TitleBarHelper.UpdateTitleBar(this.Content is FrameworkElement fe ? fe.ActualTheme : ElementTheme.Default);
    }

    private void Dispose()
    {
        settings.ColorValuesChanged -= Settings_ColorValuesChanged;
    }
}
