using Microsoft.UI.Xaml.Data;

namespace CenterWindow.Converters;

/// <summary>
/// Provides a value converter that determines whether a given value is <see langword="null"/>.
/// </summary>
/// <remarks>This converter is typically used in data binding scenarios to convert an object to a boolean value.
/// It returns <see langword="true"/> if the input value is not <see langword="null"/>; otherwise, it returns <see
/// langword="false"/>. The <c>ConvertBack</c> method is not implemented and will throw a <see
/// cref="NotImplementedException"/> if called.</remarks>
public partial class NullToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value != null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
