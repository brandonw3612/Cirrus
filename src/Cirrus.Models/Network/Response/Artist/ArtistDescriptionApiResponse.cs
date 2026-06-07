using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cirrus.Models.Network.Response.Artist;

public class ArtistDescriptionApiResponse : MusicApiResponse, IJsonOnDeserialized
{
    [JsonIgnore] public List<(string Title, string Content)> Introduction { get; private set; } = [];
    
    [JsonPropertyName("briefDesc")] public string? BriefDescription { get; init; }
    
    [JsonInclude] [JsonExtensionData] internal Dictionary<string, JsonElement> ExtensionData { get; set; } = new(); 
    
    void IJsonOnDeserialized.OnDeserialized()
    {
        if (ExtensionData.TryGetValue("introduction", out var introductionElem)
            && introductionElem.ValueKind is JsonValueKind.Array)
        {
            Introduction = introductionElem.EnumerateArray().Select(static elem =>
            {
                var title = elem.TryGetProperty("ti", out var titleElem)
                            && titleElem is {ValueKind: JsonValueKind.String} 
                    ? titleElem.GetString() ?? string.Empty
                    : string.Empty;
                var content = elem.TryGetProperty("txt", out var contentElem)
                              && contentElem is {ValueKind: JsonValueKind.String}
                    ? contentElem.GetString() ?? string.Empty
                    : string.Empty;
                return (title, content);
            }).ToList();
        }
    }
}