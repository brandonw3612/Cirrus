using System.Text.Json.Serialization;
using Cirrus.Models.Network.Track;

namespace Cirrus.Models.Network.Response.Playlist;

/// <summary>
/// Response for API playlist/intelligent-list.
/// </summary>
public class IntelligentPlaylistApiResponse : MusicApiResponse, IJsonOnDeserialized
{
    /// <summary>
    /// Tracks on the intelligent list.
    /// </summary>
    [JsonIgnore] public List<TrackDetail> Tracks { get; private set; } = new();

    [JsonInclude] [JsonPropertyName("data")] internal List<IntelligentPlaylistApiResponseData>? Data { get; init; }
    
    void IJsonOnDeserialized.OnDeserialized()
    {
        if (Data is null) return;
        Tracks = Data.Select(d => d.Track).OfType<TrackDetail>().ToList();
    }
    
    public class IntelligentPlaylistApiResponseData
    {
        [JsonPropertyName("id")] public ulong TrackId { get; init; }
        [JsonPropertyName("songInfo")] public TrackDetail? Track { get; init; }
    }
}