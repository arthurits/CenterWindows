using System.Diagnostics;
using CenterWindow.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace CenterWindow.ViewModels;

public partial class SelectWindowViewModel : ObservableRecipient
{
    private readonly IWindowCenterService _centerService;
    private readonly IMouseHookService _mouseHook;

    [ObservableProperty]
    public partial bool IsLeftButtonDown { get; set; } = false;

    private readonly string _defaultImagePath;
    private readonly string _clickedImagePath;

    [ObservableProperty]
    public partial ImageSource CurrentImage { get; set; } = null!;


    public SelectWindowViewModel(
        IWindowCenterService centerService,
        IMouseHookService mouseHook)
    {
        // Services
        _centerService = centerService;
        _mouseHook = mouseHook;
        _mouseHook.MouseMoved += OnMouseMoved;

        // Initialize the image sources. This could be read from a settings file.
        _defaultImagePath = "ms-appx:///Assets/Select window - 24x24 - Finder home.svg";
        _clickedImagePath = "ms-appx:///Assets/Select window - 24x24 - Finder gone.svg";

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
        //// Alterna rutas
        //    const string defaultPath = "ms-appx:///Assets/Default.svg";
        //    const string clickedPath = "ms-appx:///Assets/Clicked.svg";
        //    const string png1 = "ms-appx:///Assets/Default.png";
        //    const string png2 = "ms-appx:///Assets/Clicked.png";

        //    var currentUri = CurrentImage is SvgImageSource
        //        ? ((SvgImageSource)CurrentImage).UriSource.ToString()
        //        : ((BitmapImage)CurrentImage).UriSource.ToString();

        //    // Decide siguiente ruta (puedes ajustar la lógica a tus nombres)
        //    var nextPath = currentUri.EndsWith(".svg")
        //        ? (currentUri.Contains("Default") ? clickedPath : defaultPath)
        //        : (currentUri.Contains("Default") ? png2 : png1);

        CurrentImage = CreateImageSource(IsLeftButtonDown ? _clickedImagePath : _defaultImagePath);
    }

    private void OnMouseMoved(object? sender, MouseMoveEventArgs e)
    {
        // Si necesitas actualizar propiedades enlazadas a UI, despacha al hilo principal:
        _ = DispatcherQueue
            .GetForCurrentThread()
            .TryEnqueue(() =>
            {
                // Ejemplo: guardas la posición para mostrar en un TextBlock
                Debug.WriteLine($"Window handle: {e.HWnd} & Mouse moved to: {e.X}, {e.Y}");
                //CurrentMousePosition = $"{e.Point.X}, {e.Point.Y}";
            });
    }

        [RelayCommand]
    private async Task OnLeftButtonDownAsync(PointerRoutedEventArgs args)
    {
        //// Alterna rutas
        //const string defaultPath = "ms-appx:///Assets/Default.svg";
        //const string clickedPath = "ms-appx:///Assets/Clicked.svg";
        //const string png1 = "ms-appx:///Assets/Default.png";
        //const string png2 = "ms-appx:///Assets/Clicked.png";

        //var currentUri = CurrentImage is SvgImageSource
        //    ? ((SvgImageSource)CurrentImage).UriSource.ToString()
        //    : ((BitmapImage)CurrentImage).UriSource.ToString();

        //// Decide siguiente ruta (puedes ajustar la lógica a tus nombres)
        //var nextPath = currentUri.EndsWith(".svg")
        //    ? (currentUri.Contains("Default") ? clickedPath : defaultPath)
        //    : (currentUri.Contains("Default") ? png2 : png1);

        //CurrentImage = CreateImageSource(nextPath);
        
        try
        {
            Debug.WriteLine("Left button down event triggered.");
            IsLeftButtonDown = true;
            _mouseHook.CaptureMouse(true);
            //// Set the mouse hook to capture the window under the cursor
            //var hWnd = await _mouseHook.CaptureWindowUnderCursorAsync();
            //if (hWnd != IntPtr.Zero)
            //{
            //    _centerService.CenterWindow(hWnd, 255);
            //}
        }
        catch (TaskCanceledException)
        {
        }
    }

    [RelayCommand]
    private async Task OnLeftButtonUpAsync(PointerRoutedEventArgs args)
    {
        try
        {
            Debug.WriteLine("Left button up event triggered.");
            IsLeftButtonDown = false;
            await _mouseHook.CaptureMouse(false);
            //if (hWnd != IntPtr.Zero)
            //{
            //    _centerService.CenterWindow(hWnd, 255);
            //}
        }
        catch (TaskCanceledException)
        {
        }

    }

    /// <summary>
    /// Change the image when the left button state changes.
    /// </summary>
    partial void OnIsLeftButtonDownChanged(bool oldValue, bool newValue)
    {
        Debug.WriteLine($"IsLeftButtonDown changed from {oldValue} to {newValue}");
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
