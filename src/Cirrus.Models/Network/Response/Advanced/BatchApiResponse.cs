using System.Text.Json;
using System.Text.Json.Serialization;
using ModelsSerializationContext = Cirrus.Models.Serialization.ModelsSerializationContext;

namespace Cirrus.Models.Network.Response.Advanced;

public class BatchApiResponse : MusicApiResponse
{
    [JsonExtensionData] internal Dictionary<string, JsonElement> ExtensionData { get; set; } = new();

    public MusicApiResponse? GetResponse(string requestUrl, Type responseType)
    {
        if (!ExtensionData.TryGetValue(requestUrl, out var rawJsonData)) return null;
        return rawJsonData.Deserialize(responseType, ModelsSerializationContext.Default) as MusicApiResponse;
    }
}