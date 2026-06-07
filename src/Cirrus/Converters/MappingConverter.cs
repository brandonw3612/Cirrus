using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;

namespace Cirrus.Converters;

internal sealed class MappingEntry
{
    public object? Input { get; set; }
    public object? Output { get; set; }
}

[ContentProperty(Name = nameof(MappingEntries))]
internal sealed partial class MappingConverter : IValueConverter
{
    public IList<MappingEntry> MappingEntries { get; } = new List<MappingEntry>();

    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        foreach (var entry in MappingEntries)
        {
            if (entry.Input is null) continue;
            if (entry.Input.Equals(value)) return entry.Output;
        }
        return null;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return (from entry in MappingEntries where entry.Output == value select entry.Input).FirstOrDefault();
    }
}
