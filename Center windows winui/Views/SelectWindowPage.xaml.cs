using CenterWindow.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace CenterWindow.Views;

public sealed partial class SelectWindowPage : Page
{
    public SelectWindowViewModel ViewModel {get;}

    public SelectWindowPage()
    {
        ViewModel = App.GetService<SelectWindowViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
