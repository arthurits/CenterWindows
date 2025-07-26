using System.Collections.ObjectModel;
using CenterWindow.Contracts.Services;
using CenterWindow.Models;
using CenterWindow.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Media.AppBroadcasting;

namespace CenterWindow.ViewModels;

public partial class PropertyItem(string key, string value, string iconPath) : ObservableObject
{
    [ObservableProperty]
    public partial string Key { get; set; } = key;

    [ObservableProperty]
    public partial string Value { get; set; } = value;

    [ObservableProperty]
    public partial string IconPath { get; set; } = iconPath;

    [ObservableProperty]
    public partial bool IsLastItem { get; set; } = false;
}

public partial class SelectWindowViewModel : ObservableRecipient
{
    private readonly IWindowCenterService _centerService;
    private readonly IMouseHookService _mouseHook;
    private readonly AppSettings _appSettings;

    [ObservableProperty]
    public partial bool IsLeftButtonDown { get; set; } = false;

    private readonly string _defaultImagePath;
    private readonly string _clickedImagePath;
    private readonly string _cursorPath;

    [ObservableProperty]
    public partial ImageSource CurrentImage { get; set; } = null!;

    [ObservableProperty]
    public partial string WindowTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string WindowHandle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string WindowClassName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string WindowDimensions { get; set; } = string.Empty;

    public ObservableCollection<PropertyItem> WindowPropertiesCollection { get; } = [];

    public SelectWindowViewModel(
        ILocalSettingsService<AppSettings> settings,
        IWindowCenterService centerService,
        IMouseHookService mouseHook)
    {
        // Services
        _appSettings = settings.GetValues;
        _centerService = centerService;
        _mouseHook = mouseHook;
        _mouseHook.MouseMoved += OnMouseMoved;

        // Initialize the image sources. This could be read from a settings file.
        _defaultImagePath = _appSettings.SelectWindowDefaultImagePath;
        _clickedImagePath = _appSettings.SelectWindowClickedImagePath;
        _cursorPath = _appSettings.SelectWindowCursorPath;
        // Windows.ApplicationModel.Package.Current.InstalledPath + "/Assets/Config/MyFile.txt";

        // Initialize the window properties collection
        // Ejemplo: inicializa con pares clave/valor
        WindowPropertiesCollection.Add(new PropertyItem("Window text", string.Empty, string.Empty));
        WindowPropertiesCollection.Add(new PropertyItem("Window handle", string.Empty, string.Empty));
        WindowPropertiesCollection.Add(new PropertyItem("Window class name", string.Empty, string.Empty));
        WindowPropertiesCollection.Add(new PropertyItem("Window dimensions", string.Empty, string.Empty));

        // Set the initial image
        ToggleImage();
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Unsubscribe from events
            _mouseHook.MouseMoved -= OnMouseMoved;
        }
    }

    private void ToggleImage()
    {
        CurrentImage = CreateImageSource(IsLeftButtonDown ? _clickedImagePath : _defaultImagePath);
    }

    private void OnMouseMoved(object? sender, MouseMoveEventArgs e)
    {
        // Dispatch to the UI thread to avoid cross-thread operation exceptions
        _ = DispatcherQueue
            .GetForCurrentThread()
            .TryEnqueue(() =>
            {
                // Get the window information from the mouse hook event
                WindowHandle = e.HWnd.ToString();
                WindowTitle = e.WindowText;
                WindowClassName = e.ClassName;
                WindowDimensions = $"{e.Width}x{e.Height} at {e.X}, {e.Y}";
                // Update the properties collection
                WindowPropertiesCollection[0].Value = e.WindowText;
                WindowPropertiesCollection[1].Value = e.HWnd.ToString();
                WindowPropertiesCollection[2].Value = e.ClassName;
                WindowPropertiesCollection[3].Value = $"{e.Width}x{e.Height} at {e.X}, {e.Y}";

                // Force rebinding of the properties collection in the UI
                //OnPropertyChanged(nameof(WindowPropertiesCollection));

                // OnLanguageChanged()
                //var localizedKeys = localizationService.GetLocalizedKeys();
                //for (int i = 0; i < WindowPropertiesCollection.Count; i++)
                //    WindowPropertiesCollection[i].Key = localizedKeys[i];
            });
    }

        [RelayCommand]
    private async Task OnLeftButtonDownAsync(PointerRoutedEventArgs args)
    {
        IsLeftButtonDown = true;
        _mouseHook.CaptureMouse(Path.GetFullPath(_cursorPath), true, true);
    }

    [RelayCommand]
    private async Task OnLeftButtonUpAsync(PointerRoutedEventArgs args)
    {
        IsLeftButtonDown = false;
        _mouseHook.ReleaseMouse();
        if (int.TryParse(WindowHandle, out var handle) && handle != 0)
        {
            _centerService.CenterWindow((IntPtr)handle, 255);
        }
    }

    /// <summary>
    /// Change the image when the left button state changes.
    /// </summary>
    partial void OnIsLeftButtonDownChanged(bool oldValue, bool newValue)
    {
        ToggleImage();
    }

    private ImageSource CreateImageSource(string uri)
    {
        if (uri.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
        {
            // SvgImageSource está en Microsoft.UI.Xaml.Media.Imaging
            return new SvgImageSource(new Uri(uri));
        }
        else
        {
            return new BitmapImage(new Uri(uri));
        }
    }
}
