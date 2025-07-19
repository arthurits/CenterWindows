using CenterWindow.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace CenterWindow.Views;

public sealed partial class ListWindowsPage : Page
{
    public ListWindowsViewModel ViewModel { get; }

    public ListWindowsPage()
    {
        ViewModel = App.GetService<ListWindowsViewModel>();
        DataContext = ViewModel;
        InitializeComponent();

        // Set data context for the page
        ViewModel.LoadWindows();
    }
}
