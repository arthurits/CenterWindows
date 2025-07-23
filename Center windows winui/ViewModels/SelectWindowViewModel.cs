using CenterWindow.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace CenterWindow.ViewModels;

public partial class SelectWindowViewModel : ObservableRecipient
{
    private readonly IWindowCenterService _centerService;
    private readonly IMouseHookService _mouseHook;

    [ObservableProperty]
    public partial ImageSource CurrentImage { get; set; }
    

    public SelectWindowViewModel(
        IWindowCenterService centerService,
        IMouseHookService mouseHook)
    {
        // Services
        _centerService = centerService;
        _mouseHook = mouseHook;

        CurrentImage = CreateImageSource("ms-appx:///Assets/Select window - 24x24 - Finder home.svg");
    }

    [RelayCommand]
    private void ToggleImage()
    {
        // Alterna rutas
            const string defaultPath = "ms-appx:///Assets/Default.svg";
            const string clickedPath = "ms-appx:///Assets/Clicked.svg";
            const string png1 = "ms-appx:///Assets/Default.png";
            const string png2 = "ms-appx:///Assets/Clicked.png";

            var currentUri = CurrentImage is SvgImageSource
                ? ((SvgImageSource)CurrentImage).UriSource.ToString()
                : ((BitmapImage)CurrentImage).UriSource.ToString();

            // Decide siguiente ruta (puedes ajustar la lógica a tus nombres)
            var nextPath = currentUri.EndsWith(".svg")
                ? (currentUri.Contains("Default") ? clickedPath : defaultPath)
                : (currentUri.Contains("Default") ? png2 : png1);

            CurrentImage = CreateImageSource(nextPath);
    }

    [RelayCommand]
    private async Task OnLeftClickAsync(PointerRoutedEventArgs args)
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
            // Set the mouse hook to capture the window under the cursor
            var hWnd = await _mouseHook.CaptureWindowUnderCursorAsync();
            if (hWnd != IntPtr.Zero)
            {
                _centerService.CenterWindow(hWnd, 255);
            }
        }
        catch (TaskCanceledException)
        {
        }
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
