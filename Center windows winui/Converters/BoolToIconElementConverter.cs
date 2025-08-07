using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace CenterWindow.Converters;
internal partial class BoolToIconElementConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var isOn = (bool)value;

        if (isOn)
        {
            return new ImageIcon
            {
                Source = new SvgImageSource(new Uri("ms-appx:///Assets/Finder - 32x32.svg")),
                Width = 20,
                Height = 20
            };
            
        }
        else
        {
            return new FontIcon
            {
                Glyph = "\uE8B0",
                FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets"),
                Width = 20,
                Height = 20
            };
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
