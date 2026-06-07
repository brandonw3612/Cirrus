using System.Text.Json.Serialization;
using Cirrus.Models.Network.Track;

namespace Cirrus.Models.Network.Response.Account;

/// <summary>
/// Response for API account/daily-mix.
/// </summary>
public class DailyMixApiResponse : MusicApiResponse, IJsonOnDeserialized
{
    /// <summary>
    /// Tracks in the current user's daily mix playlist.
    /// </summary>
    [JsonIgnore] public List<TrackDetail> Tracks { get; private set; } = new();

    [JsonInclude] [JsonPropertyName("data")] internal DailyMixApiResponseTracksData? TracksData { get; init; }
 
    void IJsonOnDeserialized.OnDeserialized()
    {
        if (TracksData is null) return;
        Tracks = TracksData.Tracks;
    }
    
    public class DailyMixApiResponseTracksData
    {
        [JsonPropertyName("dailySongs")] public List<TrackDetail> Tracks { get; init; } = new();
    }
}