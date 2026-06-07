using System.Text.Json.Serialization;
using Cirrus.Models.Network.MusicVideo;
using Cirrus.Models.Shared.Search;

namespace Cirrus.Models.Network.Response.Search;

/// <summary>
/// Response for API search/cloud-search when search target is <see cref="SearchTarget.MusicVideo"/>.
/// </summary>
public class MusicVideoSearchApiResponse : SearchApiResponse, IJsonOnDeserialized
{
    /// <summary>
    /// Music videos matching with the keyword.
    /// </summary>
    [JsonIgnore] public List<MusicVideoDetail> MusicVideos { get; private set; } = new();
    
    /// <summary>
    /// Total count of music videos that match with the keyword.
    /// </summary>
    [JsonIgnore] public int TotalCount { get; private set; }

    /// <summary>
    /// Whether there are more music videos after the current page.
    /// </summary>
    [JsonIgnore] public bool HasMore { get; private set; }
    
    [JsonInclude] [JsonPropertyName("result")] internal MusicVideoSearchResult? Result { get; init; }

    void IJsonOnDeserialized.OnDeserialized()
    {
        if (Result is null) return;
        MusicVideos = Result.MusicVideos;
        TotalCount = Result.TotalCount;
        HasMore = Result.HasMore;
    }

    public class MusicVideoSearchResult
    {
        [JsonPropertyName("mvCount")] public int TotalCount { get; init; }
        [JsonPropertyName("mvs")] public List<MusicVideoDetail> MusicVideos { get; init; } = new();
        [JsonPropertyName("hasMore")] public bool HasMore { get; init; }
    }
}