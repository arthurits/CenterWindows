using CenterWindow.Models;
using CenterWindow.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;

namespace CenterWindow.Views;

public sealed partial class ListWindowsPage : Page
{
    public ListWindowsViewModel ViewModel
    {
        get;
    }

    public ListWindowsPage()
    {
        ViewModel = App.GetService<ListWindowsViewModel>();
        DataContext = ViewModel;
        InitializeComponent();

        // Set data context for the page
        ViewModel.LoadWindows();
    }

    private void ItemGrid_Tapped(object sender, TappedRoutedEventArgs e)
    {
        var grid = (FrameworkElement)sender;

        // Select the corresponding ListView item in the ViewModel
        if (grid.DataContext is WindowModel window)
        {
            ViewModel.SelectedWindow = window;
        }

        // Get the ContextFlyout from the grid and show it at the tapped position
        if (grid.ContextFlyout is FlyoutBase flyout)
        {
            // Show the flyout at the tapped position
            var pt = e.GetPosition(grid);
            flyout.ShowAt(grid, new FlyoutShowOptions
            {
                ShowMode = FlyoutShowMode.Transient,
                Placement = FlyoutPlacementMode.Bottom,
                Position = pt
            });
        }

        e.Handled = true;
    }
}
