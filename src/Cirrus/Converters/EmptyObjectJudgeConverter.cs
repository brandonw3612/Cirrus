using System.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Cirrus.Converters;

internal sealed partial class EmptyObjectJudgeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, string language)
    {
        var judge = value switch
        {
            null => false,
            string s => !string.IsNullOrWhiteSpace(s.Replace("\r\n", string.Empty).Replace("\n", string.Empty)),
            IEnumerable e => e.GetEnumerator().MoveNext(),
            decimal d => d > 0,
            _ => true
        };
        return targetType == typeof(Visibility) ? judge switch
        {
            true => Visibility.Visible,
            false => Visibility.Collapsed
        } : judge;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}