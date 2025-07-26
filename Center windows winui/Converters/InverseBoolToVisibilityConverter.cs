using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace CenterWindow.Converters;
public partial class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
      => (value is bool b && b)
           ? Visibility.Collapsed
           : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
