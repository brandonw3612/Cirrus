using System.Text.Json.Serialization;

namespace Cirrus.Models.Network.Public;

/// <summary>
/// Currently trending keyword appeared in user search queries.
/// </summary>
public class TrendingSearchKeyword
{
    /// <summary>
    /// Trending search keyword.
    /// </summary>
    [JsonPropertyName("searchWord")] public string Keyword { get; init; } = string.Empty;
    
    /// <summary>
    /// Popularity of the keyword.
    /// </summary>
    [JsonPropertyName("score")] public int Popularity { get; init; }
    
    /// <summary>
    /// Extended content of the keyword, aka recommendation reason.
    /// </summary>
    [JsonPropertyName("content")] public string? ExtendedContent { get; init; }

    public Business.Search.TrendingSearchKeyword ToBusinessModel() => new(Keyword, ExtendedContent);
}