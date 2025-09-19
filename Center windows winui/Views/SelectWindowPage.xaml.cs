using CenterWindow.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace CenterWindow.Views;

public sealed partial class SelectWindowPage : Page
{
    private const double _canvasOverlayMarginFromEdge = 24;

    public SelectWindowViewModel ViewModel {get;}

    public SelectWindowPage()
    {
        ViewModel = App.GetService<SelectWindowViewModel>();
        InitializeComponent();
        DataContext = ViewModel;

        // Listen for pointer pressed events on the SelectWindow element even if the event is handled internally
        SelectWindow.AddHandler(
            UIElement.PointerPressedEvent,
            new PointerEventHandler(OnSelectWindow_PointerPressed),
            handledEventsToo: true);

        // Listen for pointer released events on the SelectWindow element even if the event is handled internally
        SelectWindow.AddHandler(
          UIElement.PointerReleasedEvent,
          new PointerEventHandler(OnSelectWindow_PointerReleased),
          handledEventsToo: true);

        // Set initial position of canvas RestoreCursorCanvas
        CanvasOverlay.Loaded += CanvasOverlay_Loaded;
    }

    private void CanvasOverlay_Loaded(object sender, RoutedEventArgs e)
    {
        var left = ActualWidth - RestoreCursorPanel.ActualWidth - _canvasOverlayMarginFromEdge;
        var top = ActualHeight - RestoreCursorPanel.ActualHeight - _canvasOverlayMarginFromEdge;

        // Avoid negative values
        ViewModel.PanelLeft = left >= _canvasOverlayMarginFromEdge ? left : _canvasOverlayMarginFromEdge;
        ViewModel.PanelTop = top >= _canvasOverlayMarginFromEdge ? top : _canvasOverlayMarginFromEdge;

        // We want this to be a one-time operation: the initial positioning of the canvas overlay
        CanvasOverlay.Loaded -= CanvasOverlay_Loaded;
    }

    private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ScrollViewPage.Height = ActualHeight;
        ScrollViewPage.Width = ActualWidth;
    }

    private void RestoreCursor_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
    {
        // Ajusta la posición en el ViewModel
        ViewModel.PanelLeft += e.Delta.Translation.X;
        ViewModel.PanelTop += e.Delta.Translation.Y;
    }

    private void OnSelectWindow_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        // Filter out non-left button clicks
        var props = e.GetCurrentPoint(null).Properties;
        if (!props.IsLeftButtonPressed)
        {
            return;
        }
        // Execute the command and handle the left click
        if (ViewModel.LeftButtonDownCommand.CanExecute(e))
        {
            ViewModel.LeftButtonDownCommand.Execute(e);
        }
    }

    private void OnSelectWindow_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        var props = e.GetCurrentPoint(null).Properties;

        // Filter out non-left button releases
        if (props.IsLeftButtonPressed)
        {
            return;
        }

        // Execute the command and handle the left click
        if (ViewModel.LeftButtonUpCommand.CanExecute(e))
        {
            ViewModel.LeftButtonUpCommand.Execute(e);
        }
    }
}
