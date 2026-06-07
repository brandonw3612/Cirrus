using System.Collections;
using Cirrus.Models.Abstract.Primitives;
using Microsoft.UI.Xaml.Data;

namespace Cirrus.Converters;

public partial class MultipleEntityNamesConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not IEnumerable enumerable) return "{InvalidInput}";
        var names = enumerable.OfType<INamedEntity>().Select(static ne => ne.EntityName).ToArray();
        return names.Length switch
        {
            0 => string.Empty,
            1 => names[0],
            _ => string.Join(", ", names[..^1]) + " & " + names[^1]
        };
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }
}