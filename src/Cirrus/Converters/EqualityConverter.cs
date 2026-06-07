using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Cirrus.Converters;

internal sealed partial class EqualityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object parameter, string language)
    {
        var isEqual = parameter switch
        {
            null => value is null,
            _ => parameter.Equals(value),
        };
        if (targetType == typeof(bool)) return isEqual;
        if (targetType == typeof(Visibility)) return isEqual ? Visibility.Visible : Visibility.Collapsed;
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return new NotSupportedException();
    }
}