using System.Collections.ObjectModel;
using CenterWindow.Contracts.Services;
using CenterWindow.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

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

public partial class SelectWindowViewModel : ObservableRecipient, IDisposable
{
    private readonly IWindowCenterService _centerService;
    private readonly IMouseHookService _mouseHook;
    private readonly AppSettings _appSettings;
    private readonly ILocalizationService _localizationService;
    private readonly IWindowHighlightService _highlightService;

    private readonly string _defaultImagePath;
    private readonly string _clickedImagePath;
    private readonly string _cursorPath;
    private IntPtr _selectedWindowHandle = IntPtr.Zero;
    private IntPtr _lastHighlightedHwnd = IntPtr.Zero;

    [ObservableProperty]
    public partial bool IsLeftButtonDown { get; set; } = false;
    
    [ObservableProperty]
    public partial ImageSource CurrentImage { get; set; } = null!;

    public ObservableCollection<PropertyItem> WindowPropertiesCollection { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StrTransparencyText))]
    public partial int Transparency { get; set; }       // Set in constructor. Default value -> 255

    private byte Alpha => (byte)Math.Clamp(Transparency, 0, 255);
    public string StrTransparencyText => $"{StrTransparencyHeader}: {Alpha}";

    // Properties for the window selection and highlighter
    private bool _showHighlight;
    private string _borderColor;
    private uint _colorRef;
    private int _borderThickness;
    private int _borderRadius;
    private bool _selectChild;
    private bool _changeCursor;

    public SelectWindowViewModel(
        ILocalSettingsService<AppSettings> settings,
        IWindowCenterService centerService,
        IMouseHookService mouseHook,
        IWindowHighlightService highlightService,
        ILocalizationService localizationService)
    {
        // Services
        _appSettings = settings.GetValues;
        settings.SettingChanged += OnPropertyChanged;

        _showHighlight = _appSettings.ShowHighlight;
        _borderColor = _appSettings.BorderColor ?? "#FFFF0000"; // Default to red if null
        _colorRef = StringColorToColorRef(_borderColor);
        _borderThickness = _appSettings.BorderThickness;
        _borderRadius = _appSettings.BorderRadius;
        _selectChild = _appSettings.SelectChildWindows;
        _changeCursor = _appSettings.ChangeCursor;

        _centerService = centerService;
        _mouseHook = mouseHook;
        _mouseHook.MouseMoved += OnMouseMoved;
        _highlightService  = highlightService;
        _localizationService = localizationService;
        _localizationService.LanguageChanged += OnLanguageChanged;

        // Initialize the image sources. This could be read from a settings file.
        _defaultImagePath = _appSettings.SelectWindowDefaultImagePath;
        _clickedImagePath = _appSettings.SelectWindowClickedImagePath;
        _cursorPath = _appSettings.SelectWindowCursorPath;
        // Windows.ApplicationModel.Package.Current.InstalledPath + "/Assets/Config/MyFile.txt";

        // Initialize the window properties collection
        WindowPropertiesCollection.Add(new PropertyItem("Text", string.Empty, string.Empty));
        WindowPropertiesCollection.Add(new PropertyItem("Handle", string.Empty, string.Empty));
        WindowPropertiesCollection.Add(new PropertyItem("Module name", string.Empty, string.Empty));
        WindowPropertiesCollection.Add(new PropertyItem("Class name", string.Empty, string.Empty));
        WindowPropertiesCollection.Add(new PropertyItem("Location", string.Empty, string.Empty));
        WindowPropertiesCollection.Add(new PropertyItem("Dimensions", string.Empty, string.Empty));
        WindowPropertiesCollection.Last().IsLastItem = true;

        // Set the initial image
        ToggleImage();

        // Load string resources into binding variables for the UI
        OnLanguageChanged(null, EventArgs.Empty);

        // Set initial value from settings for the transparency slider
        Transparency = _appSettings.SelectWindowTransparency;
    }

    private void OnPropertyChanged(object? sender, SettingChangedEventArgs e)
    {
        // Update the properties collection when a setting changes
        switch (e.PropertyName)
        {
            case nameof(AppSettings.ShowHighlight):
                _showHighlight = (bool)(e.NewValue ?? false);
                break;
            case nameof(AppSettings.BorderColor):
                _borderColor = (string)(e.NewValue ?? "#FFFF0000"); // Default to red if null
                _colorRef = StringColorToColorRef(_borderColor);
                break;
            case nameof(AppSettings.BorderThickness):
                _borderThickness = (int)(e.NewValue ?? 3); // Default to 3 if null
                break;
            case nameof(AppSettings.BorderRadius):
                _borderRadius = (int)(e.NewValue ?? 4); // Default to 4 if null
                break;
            case nameof(AppSettings.SelectChildWindows):
                _selectChild = (bool)(e.NewValue ?? false);
                break;
            case nameof(AppSettings.ChangeCursor):
                _changeCursor = (bool)(e.NewValue ?? true);
                break;
        }
    }

    public void Dispose()
    {
        _mouseHook.MouseMoved                   -= OnMouseMoved;
        _localizationService.LanguageChanged    -= OnLanguageChanged;
        _highlightService.ClearHighlight();
        _highlightService.Dispose();
    }

    private uint StringColorToColorRef(string color)
    {
        var c = CommunityToolkit.WinUI.Helpers.ColorHelper.ToColor(color);
        return (uint)(c.R | (c.G << 8) | (c.B << 16));
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
                // Update the properties collection with information from the mouse hook event
                _selectedWindowHandle = e.HWnd;
                WindowPropertiesCollection[0].Value = e.WindowText;
                WindowPropertiesCollection[1].Value = $"{e.HWnd} (0x{e.HWnd:X})";
                WindowPropertiesCollection[2].Value = e.ModuleName;
                WindowPropertiesCollection[3].Value = e.ClassName;
                WindowPropertiesCollection[4].Value = $"{e.X}, {e.Y}";
                WindowPropertiesCollection[5].Value = $"{e.Width}x{e.Height}";

                // Force rebinding of the properties collection in the UI
                //OnPropertyChanged(nameof(WindowPropertiesCollection));

                // OnLanguageChanged()
                //var localizedKeys = localizationService.GetLocalizedKeys();
                //for (int i = 0; i < WindowPropertiesCollection.Count; i++)
                //    WindowPropertiesCollection[i].Key = localizedKeys[i];

                if (_showHighlight)
                {// If the new HWND is different from the last highlighted one, update the highlight
                    if (e.HWnd != _lastHighlightedHwnd)
                    {
                        // Clear (hide) the previous highlight
                        _highlightService.HideHighlight();

                        // If the new HWND is valid, highlight it
                        if (e.HWnd != IntPtr.Zero)
                        {
                            _highlightService.HighlightWindow(
                                e.HWnd,
                                cornerRadius: _borderRadius,
                                borderColor: _colorRef,
                                thickness: _borderThickness);
                        }

                        // Store the last highlighted HWND
                        _lastHighlightedHwnd = e.HWnd;
                    }
                }
            });
    }

    [RelayCommand]
    private void OnLeftButtonDown(PointerRoutedEventArgs args)
    {
        IsLeftButtonDown = true;
        _mouseHook.CaptureMouse(
            cursorPath: Path.GetFullPath(_cursorPath),
            changeCursor: _changeCursor,
            onlyParentWnd: !_selectChild);
    }

    [RelayCommand]
    private void OnLeftButtonUp(PointerRoutedEventArgs args)
    {
        IsLeftButtonDown = false;
        _mouseHook.ReleaseMouse();

        // Actions carriedn on the selected window
        if (_selectedWindowHandle != IntPtr.Zero)
        {
            _centerService.CenterWindow(_selectedWindowHandle);
            _centerService.SetWindowTransparency(_selectedWindowHandle, Alpha);
        }
        
        // Delete the highlight from the last selected window
        _highlightService.ClearHighlight();
        _lastHighlightedHwnd = IntPtr.Zero;
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
