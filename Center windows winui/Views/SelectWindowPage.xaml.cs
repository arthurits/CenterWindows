using CenterWindow.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.Foundation;

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
        OverlayCanvas.Loaded += OverlayCanvas_Loaded;
    }

    private void OverlayCanvas_Loaded(object sender, RoutedEventArgs e)
    {
        var left = ActualWidth - RestoreCursorPanel.ActualWidth - _canvasOverlayMarginFromEdge;
        var top = ActualHeight - RestoreCursorPanel.ActualHeight - _canvasOverlayMarginFromEdge;

        // Avoid negative values
        ViewModel.PanelLeft = left >= _canvasOverlayMarginFromEdge ? left : _canvasOverlayMarginFromEdge;
        ViewModel.PanelTop = top >= _canvasOverlayMarginFromEdge ? top : _canvasOverlayMarginFromEdge;

        // We want this to be a one-time operation: the initial positioning of the canvas overlay
        OverlayCanvas.Loaded -= OverlayCanvas_Loaded;
    }

    private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ScrollViewPage.Height = ActualHeight;
        ScrollViewPage.Width = ActualWidth;
    }

    private void RestoreCursor_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
    {
        // Offset Canvas → Page
        var canvasToPage = OverlayCanvas.TransformToVisual(PageRoot);
        Point canvasOffset = canvasToPage.TransformPoint(new Point(0, 0));

        // Horizontal (Canvas)
        double newLeft = ViewModel.PanelLeft + e.Delta.Translation.X;
        double minLeft = 0;
        double maxLeft = OverlayCanvas.ActualWidth
                       - RestoreCursorPanel.ActualWidth
                       - minLeft;
        ViewModel.PanelLeft = Math.Clamp(newLeft, minLeft, maxLeft);

        // Vertical (Page)
        double newTop = ViewModel.PanelTop + e.Delta.Translation.Y;
        double minTopPage = _canvasOverlayMarginFromEdge;
        double maxTopPage = PageRoot.ActualHeight
                           - RestoreCursorPanel.ActualHeight
                           - minTopPage;
        double minTopCanvas = minTopPage - canvasOffset.Y - 100;
        double maxTopCanvas = maxTopPage - canvasOffset.Y;
        ViewModel.PanelTop = Math.Clamp(newTop, minTopCanvas, maxTopCanvas);
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
