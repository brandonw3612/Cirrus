using Microsoft.UI.Xaml.Data;

namespace Cirrus.Converters;

internal sealed partial class LargeNumberUnitConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (!TryConvertToDecimal(value, out var decimalValue)) return value;
        string[] units = [string.Empty, "K", "M", "B", "T"];
        var unitIndex = 0;
        while (true)
        {
            if (unitIndex >= units.Length) return "∞";
            if (decimalValue > 1000)
            {
                unitIndex++;
                decimalValue /= 1000;
                continue;
            }
            return $"{decimalValue:G3}{units[unitIndex]}";
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotSupportedException();
    }
    
    private static bool TryConvertToDecimal(object? value, out decimal result)
    {
        switch (value)
        {
            case sbyte sbyteValue: result = sbyteValue; return true;
            case byte byteValue: result = byteValue; return true;
            case short shortValue: result = shortValue; return true;
            case ushort ushortValue: result = ushortValue; return true;
            case int intValue: result = intValue; return true;
            case uint uintValue: result = uintValue; return true;
            case long longValue: result = longValue; return true;
            case ulong ulongValue: result = ulongValue; return true;
            case float floatValue: result = (decimal)floatValue; return true;
            case double doubleValue: result = (decimal)doubleValue; return true;
            case decimal decimalValue: result = decimalValue; return true;
            default: result = default; return false;
        }
    }
}