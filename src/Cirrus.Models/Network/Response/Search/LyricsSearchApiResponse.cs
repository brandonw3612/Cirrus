using System.Text.Json.Serialization;
using Cirrus.Models.Network.Track;
using Cirrus.Models.Shared.Search;

namespace Cirrus.Models.Network.Response.Search;

/// <summary>
/// Response for API search/cloud-search when search target is <see cref="SearchTarget.Lyrics"/>.
/// </summary>
public class LyricsSearchApiResponse : SearchApiResponse, IJsonOnDeserialized
{
    /// <summary>
    /// Tracks that contain lyrics matching with the keyword.
    /// </summary>
    [JsonIgnore] public List<LyricsMatchTrack> Tracks { get; private set; } = new();
    
    /// <summary>
    /// Total count of tracks containing lyrics that match with the keyword.
    /// </summary>
    [JsonIgnore] public int TotalCount { get; private set; }

    /// <summary>
    /// Whether there are more tracks containing matched lyrics after the current page.
    /// </summary>
    [JsonIgnore] public bool HasMore { get; private set; }
    
    [JsonInclude] [JsonPropertyName("result")] internal LyricsSearchResult? Result { get; init; }

    void IJsonOnDeserialized.OnDeserialized()
    {
        if (Result is null) return;
        Tracks = Result.Tracks ?? [];
        TotalCount = Result.TotalCount;
        HasMore = Result.HasMore;
    }

    public class LyricsSearchResult
    {
        [JsonPropertyName("songs")] public List<LyricsMatchTrack>? Tracks { get; init; } = new();
        [JsonPropertyName("songCount")] public int TotalCount { get; init; }
        [JsonPropertyName("hasMore")] public bool HasMore { get; init; }
    }
}