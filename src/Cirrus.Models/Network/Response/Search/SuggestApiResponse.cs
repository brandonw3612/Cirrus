using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cirrus.Models.Network.Response.Search;

/// <summary>
/// Response for API search/suggest.
/// </summary>
public class SuggestApiResponse : MusicApiResponse, IJsonOnDeserialized
{
    /// <summary>
    /// Keyword suggestions based on the keyword given by the user.
    /// </summary>
    [JsonIgnore] public List<string> Keywords { get; private set; } = new();
    
    [JsonInclude] [JsonExtensionData] internal Dictionary<string, JsonElement> ExtensionData { get; set; } = new();

    void IJsonOnDeserialized.OnDeserialized()
    {
        Keywords.Clear();
        if (!ExtensionData.TryGetValue("result", out var result)
            || !result.TryGetProperty("allMatch", out var allMatchElem)
            || allMatchElem is not {ValueKind: JsonValueKind.Array}) return;
        Keywords = allMatchElem.EnumerateArray().Select(static t =>
            t.TryGetProperty("keyword", out var keywordElem) && keywordElem is {ValueKind: JsonValueKind.String}
                ? keywordElem.GetString()
                : null
        ).OfType<string>().ToList();
    }
}

