using System.Text.Json.Serialization;
using Cirrus.Models.Network.Public;

namespace Cirrus.Models.Network.Response.Search;

/// <summary>
/// Response for API search/trend.
/// </summary>
public class TrendApiResponse : MusicApiResponse
{
    /// <summary>
    /// Trending search keyword entries.
    /// </summary>
    [JsonPropertyName("data")] public List<TrendingSearchKeyword> Entries { get; init; } = new();
}