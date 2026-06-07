using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Data;

namespace Cirrus.Converters;

/// <summary>
/// Converter for literal or formatted language resources.
/// <example>
/// <para><b>Literal</b></para>
/// <para>value (string):  ResourceId, whose corresponding string is a literal</para>
/// <para>parameter: null</para>
/// <para>* For most cases, we suggest using "x:Uid" directives.</para>
/// <para><b>Formatted</b></para>
/// <para>parameter (string): ResourceId, whose corresponding string is a format string</para>
/// <para>value (object): content passed to the format</para>
/// </example>
/// </summary>
internal sealed partial class LanguageResourceConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        // Literal
        if (parameter is null)
        {
            return value is not string id
                ? "{Invalid Type: Resource ID}"
                : id.GetLocalized() ?? "{Invalid Resource ID}";
        }
        if (parameter is not string resourceId) return "{Invalid Type: Resource ID}";
        return resourceId.GetLocalized() is not { } formatString
            ? "{Invalid Resource ID}"
            : string.Format(formatString, value);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        return string.Empty;
    }
}
