using CenterWindow.Models;
using CenterWindow.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace CenterWindow.Views;

public sealed partial class ListWindowsPage : Page
{
    public ListWindowsViewModel ViewModel { get; }
    private bool _cancelNextClose = false;

    public ListWindowsPage()
    {
        ViewModel = App.GetService<ListWindowsViewModel>();
        DataContext = ViewModel;
        InitializeComponent();

        //ViewModel.RefreshWindows();
    }

    /// <summary>
    /// Handles the <see cref="FrameworkElement.Loaded"/> event for an <see cref="AppBarToggleButton"/> and <see cref="AppBarButton"/>. Adjusts the
    /// alignment and size of specific child elements within the control's visual tree.
    /// </summary>
    /// <remarks>This method modifies the visual appearance of the <see cref="AppBarToggleButton"/> and <see cref="AppBarButton"/> by: <list
    /// type="bullet"> <item> <description>Centering the vertical alignment of a <see cref="TextBlock"/> named
    /// "TextLabel" within the control's template.</description> </item> <item> <description>Setting the height and
    /// width of a <see cref="Viewbox"/> named "ContentViewbox" to 24 units.</description> </item> </list> Ensure that
    /// the control's template contains elements with the expected names ("TextLabel" and "ContentViewbox") for this
    /// method to function correctly.</remarks>
    /// <param name="sender">The source of the event, expected to be an <see cref="AppBarToggleButton"/> or an <see cref="AppBarButton"/>.</param>
    /// <param name="e">The event data associated with the <see cref="FrameworkElement.Loaded"/> event.</param>
    /// <seealso href="https://github.com/microsoft/microsoft-ui-xaml/blob/main/src/controls/dev/CommonStyles/AppBarToggleButton_themeresources.xaml"/>
    /// <seealso href="https://github.com/microsoft/microsoft-ui-xaml/blob/main/src/controls/dev/CommonStyles/AppBarButton_themeresources.xaml"/>
    private void AppBarButtonAndToggle_Loaded(object sender, RoutedEventArgs e)
    {
        // Make sure it's a Control so that we can check the template and the visual tree
        if (sender is Control ctrl)
        {
            // Make sure the code is only executed once
            ctrl.Loaded -= AppBarButtonAndToggle_Loaded;
            ctrl.ApplyTemplate();

            // The control's template should contain a root element that is a FrameworkElement
            if (VisualTreeHelper.GetChild(ctrl, 0) is FrameworkElement root)
            {
                // Set the vertical alignment of the TextBlock named "TextLabel" to Center
                if (root.FindName("TextLabel") is TextBlock lbl)
                {
                    lbl.VerticalAlignment = VerticalAlignment.Center;
                }

                // Set the height and width of the Viewbox named "ContentViewbox" to 24
                if (root.FindName("ContentViewbox") is Viewbox vbx)
                {
                    vbx.Height = vbx.Width = 24;
                }
            }
        }
    }

    private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ScrollViewPage.Height = ActualHeight;
        ScrollViewPage.Width = ActualWidth;
    }

    private void DeselectWindows_Click(object sender, RoutedEventArgs e)
    {
        if (WindowsListView.SelectedItems.Count > 0)
        {
            WindowsListView.SelectedItems.Clear();
        }
    }

    private void ToggleMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        _cancelNextClose = true;
    }

    private void OptionsMenuFlyout_Closing(FlyoutBase sender, FlyoutBaseClosingEventArgs args)
    {
        // If the closing comes from a ToggleMenuFlyoutItem, we cancel the closing of the flyout.
        if (_cancelNextClose)
        {
            args.Cancel = true;
            _cancelNextClose = false;
        }
    }

    private void ItemGrid_Tapped(object sender, TappedRoutedEventArgs e)
    {
        var grid = (FrameworkElement)sender;

        // Select the corresponding ListView item in the ViewModel
        if (grid.DataContext is WindowModel window)
        {
            //ViewModel.SelectedWindow = window;
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

        // Indicate that the event has been handled so that it doesn't get to the ListView control
        e.Handled = true;
    }

    private void ItemGrid_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        var grid = (FrameworkElement)sender;

        // Select the corresponding ListView item in the ViewModel
        if (grid.DataContext is WindowModel win)
        {
            //ViewModel.SelectedWindow = win;
        }

        // Show the context menu
        if (grid.ContextFlyout is FlyoutBase flyout)
        {
            flyout.ShowAt(grid);
        }

        //// Show the flyout menu only if the item was already selected
        //if (grid.DataContext is WindowModel win
        //    && ViewModel.SelectedWindow == win
        //    && grid.ContextFlyout is FlyoutBase flyout)
        //{
        //    flyout.ShowAt(grid);
        //}

        // Indicate that the event has been handled so that it doesn't get to the ListView control
        e.Handled = true;
    }
}
