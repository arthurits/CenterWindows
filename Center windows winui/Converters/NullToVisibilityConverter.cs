using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace CenterWindow.Converters;
public partial class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var path = value as string;
        return string.IsNullOrEmpty(path)
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}