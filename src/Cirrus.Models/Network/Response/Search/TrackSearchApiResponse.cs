using System.Text.Json.Serialization;
using Cirrus.Models.Network.Track;
using Cirrus.Models.Shared.Search;

namespace Cirrus.Models.Network.Response.Search;

/// <summary>
/// Response for API search/cloud-search when search target is <see cref="SearchTarget.Track"/>.
/// </summary>
public class TrackSearchApiResponse : SearchApiResponse, IJsonOnDeserialized
{
    /// <summary>
    /// Tracks matching with the keyword.
    /// </summary>
    [JsonIgnore] public List<TrackDetail> Tracks { get; private set; } = new();
    
    /// <summary>
    /// Total count of tracks that match with the keyword.
    /// </summary>
    [JsonIgnore] public int TotalCount { get; private set; }

    [JsonInclude] [JsonPropertyName("result")] internal TrackSearchResult? Result { get; init; }

    void IJsonOnDeserialized.OnDeserialized()
    {
        if (Result is null) return;
        Tracks = Result.Tracks;
        TotalCount = Result.TotalCount;
    }

    public class TrackSearchResult
    {
        [JsonPropertyName("songs")] public List<TrackDetail> Tracks { get; init; } = new();
        [JsonPropertyName("songCount")] public int TotalCount { get; init; }
    }
}