using CenterWindow.Contracts.Services;
using CenterWindow.Interop;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CenterWindow.Services;
public partial class MainWindowService : ObservableObject, IMainWindowService, IDisposable
{
    private readonly WindowEx _window;
    private readonly IntPtr _hWnd;

    [ObservableProperty]
    public partial int WindowLeft { get; set; }

    [ObservableProperty]
    public partial int WindowTop { get; set; }

    [ObservableProperty]
    public partial int WindowWidth { get; set; }

    [ObservableProperty]
    public partial int WindowHeight { get; set; }

    [ObservableProperty]
    public partial string Title { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial string TitleMain { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string TitleFile { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string TitleUnion { get; set; } = "-";

    public MainWindowService(WindowEx window)
    {
        // Get window objects
        _window = window;
        _hWnd = window.GetWindowHandle();

        WindowLeft   = window.AppWindow.Position.X;
        WindowTop    = window.AppWindow.Position.Y;
        WindowWidth  = window.AppWindow.Size.Width;
        WindowHeight = window.AppWindow.Size.Height;

        // Subscribe to window position and size change events
        window.SizeChanged          += OnSizeChanged;
        window.PositionChanged      += OnPositionChanged;
        window.WindowStateChanged   += OnWindowStateChanged; ;
    }

    public void Dispose()
    {
        _window.SizeChanged         -= OnSizeChanged;
        _window.PositionChanged     -= OnPositionChanged;
        _window.WindowStateChanged  -= OnWindowStateChanged;
    }

    private void OnSizeChanged(object? sender, Microsoft.UI.Xaml.WindowSizeChangedEventArgs args)
    {
        WindowWidth  = (int)args.Size.Width;
        WindowHeight = (int)args.Size.Height;
    }

    private void OnPositionChanged(object? sender, Windows.Graphics.PointInt32 e)
    {
        WindowLeft = e.X;
        WindowTop  = e.Y;
    }

    private void OnWindowStateChanged(object? sender, WindowState e)
    {
        if (e == WindowState.Minimized)
        {
            Hide();
            // Y dejamos que el servicio de bandeja muestre su icono
            // (el propio MainWindow.xaml.cs ya habrá inicializado ITrayIconService)
        }
    }

    partial void OnTitleChanged(string oldValue, string newValue)
    {
        _window.Title = newValue;
    }

    partial void OnTitleMainChanged(string oldValue, string newValue)
    {
        Title = WindowText();
    }

    partial void OnTitleFileChanged(string oldValue, string newValue)
    {
        Title = WindowText();
    }

    partial void OnTitleUnionChanged(string oldValue, string newValue)
    {
        Title = WindowText();
    }

    /// <summary>
    /// Computes the window text based on the main title, file name, and union string.
    /// </summary>
    /// <returns>The window text</returns>
    public string WindowText()
    {
        string strText;
        var strUnion = $" {TitleUnion} ";

        if (TitleFile.Length > 0)
        {
            strText = $"{strUnion}{TitleFile}";
        }
        else
        {
            var index = TitleMain.IndexOf(strUnion) > -1 ? TitleMain.IndexOf(strUnion) : TitleMain.Length;
            strText = TitleMain[index..];
        }

        return TitleMain + strText;
    }

    public void Hide()
    {
        NativeMethods.ShowWindow(_hWnd, NativeMethods.SW_HIDE);
    }

    public void Show()
    {
        // Show the window, restore it, and place it on top
        NativeMethods.ShowWindow(_hWnd, NativeMethods.SW_SHOW);
        _window.Restore();
        _window.AppWindow.MoveInZOrderAtTop();
    }
}
