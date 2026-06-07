using Microsoft.UI.Xaml.Data;

namespace Cirrus.Converters;

public partial class TimeSpanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            TimeSpan timeSpan => timeSpan.ToString(@"m\:ss"),
            double ms => TimeSpan.FromMilliseconds(ms).ToString(@"m\:ss"),
            _ => "0:00"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}