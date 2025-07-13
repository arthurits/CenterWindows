using CenterWindow.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace CenterWindow.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        DataContext = ViewModel;
        InitializeComponent();

        // Set data context for the page
        ViewModel.LoadWindows();
    }
}
