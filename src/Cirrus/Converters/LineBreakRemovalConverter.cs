using Microsoft.UI.Xaml.Data;

namespace Cirrus.Converters;

internal sealed partial class LineBreakRemovalConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is not string str ? value : str.Replace("\r\n", " ").Replace("\n", " ");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}