using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Cirrus.Converters;

internal sealed partial class NeteaseImageResizeConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        var originalUrl = value switch
        {
            string stringUrl => stringUrl,
            Uri uri => uri.ToString(),
            _ => null
        };
        if (originalUrl is null) return null;
        var size = parameter switch
        {
            string stringSize => stringSize,
            int intSize => intSize.ToString(),
            _ => null
        };
        var sizedUrl = originalUrl.StartsWith("http") ? $"{originalUrl}?param={size}y{size}" : originalUrl;
        return new BitmapImage(new(sizedUrl));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}