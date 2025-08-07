using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace CenterWindow.Converters;
public class BoolToIconElementConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var isOn = (bool)value;

        if (isOn)
        {
            // Cuando está activado: un glifo
            return new FontIcon
            {
                Glyph = "\uE8B0",
                FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets"),
                Width = 20,
                Height = 20
            };
        }
        else
        {
            // Cuando está desactivado: un SVG (BitmapIcon admite .svg en UWP 1809+)
            return new BitmapIcon
            {
                UriSource = new Uri("/Assets/Finder - 32x32.svg"),
                Width = 20,
                Height = 20
            };
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
