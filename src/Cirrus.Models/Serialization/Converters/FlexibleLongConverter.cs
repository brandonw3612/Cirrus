using System.Buffers;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cirrus.Models.Serialization.Converters;

public class FlexibleLongConverter : JsonConverter<long>
{
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number when reader.TryGetInt64(out long l) => l,
            JsonTokenType.Number => ParseRaw(GetRawText(ref reader)),
            JsonTokenType.String => ParseRaw(reader.GetString()!),
            _ => throw new JsonException($"Cannot convert {reader.TokenType} to long")
        };
    }

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);

    private static string GetRawText(ref Utf8JsonReader reader)
    {
        var span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
        return Encoding.UTF8.GetString(span);
    }

    private static long ParseRaw(string s)
    {
        var d = decimal.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture);
        return (long)decimal.Truncate(d);
    }
}