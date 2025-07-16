using CenterWindow.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace CenterWindow.Views;

public sealed partial class AboutPage : Page
{
    public AboutViewModel AboutViewModel { get; }

    public AboutPage()
    {
        InitializeComponent();
        AboutViewModel = App.GetService<AboutViewModel>();

        // Set the DataContext to the ViewModel
        DataContext = AboutViewModel;
    }
}
