using Cirrus.Models.Abstract;
using Microsoft.UI.Xaml.Data;

namespace Cirrus.Converters;

public partial class StringQueryConverter : IValueConverter
{
    public class Query : IQuery
    {
        public required string Keyword { get; init; }
    }
    
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return new Query { Keyword = value.ToString() ?? string.Empty };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}