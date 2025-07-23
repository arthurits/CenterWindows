using CenterWindow.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace CenterWindow.Views;

public sealed partial class SelectWindowPage : Page
{
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
        if (ViewModel.LeftClickCommand.CanExecute(e))
        {
            ViewModel.LeftClickCommand.Execute(e);
        }
    }
}
