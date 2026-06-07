using System.Text.Json.Serialization;
using Cirrus.Models.Network.Podcast;
using Cirrus.Models.Shared.Search;

namespace Cirrus.Models.Network.Response.Search;

/// <summary>
/// Response for API search/cloud-search when search target is <see cref="SearchTarget.PodcastChannel"/>.
/// </summary>
public class PodcastChannelSearchApiResponse : SearchApiResponse, IJsonOnDeserialized
{
    /// <summary>
    /// Podcast channels matching with the keyword.
    /// </summary>
    [JsonIgnore] public List<PodcastChannelDetail> PodcastChannels { get; private set; } = new();
    
    /// <summary>
    /// Total count of podcast channels that match with the keyword.
    /// </summary>
    [JsonIgnore] public int TotalCount { get; private set; }
    
    /// <summary>
    /// Whether there are more podcast channels after the current page.
    /// </summary>
    [JsonIgnore] public bool HasMore { get; private set; }

    [JsonInclude] [JsonPropertyName("result")] internal PodcastChannelSearchResult? Result { get; init; }

    void IJsonOnDeserialized.OnDeserialized()
    {
        if (Result is null) return;
        PodcastChannels = Result.PodcastChannels ?? [];
        TotalCount = Result.TotalCount;
        HasMore = Result.HasMore;
    }

    public class PodcastChannelSearchResult
    {
        [JsonPropertyName("djRadios")] public List<PodcastChannelDetail>? PodcastChannels { get; init; } = new();
        [JsonPropertyName("djRadiosCount")] public int TotalCount { get; init; }
        [JsonPropertyName("hasMore")] public bool HasMore { get; init; }
    }
}