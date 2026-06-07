using System.Text.Json.Serialization;
using Cirrus.Models.Network.Artist;

namespace Cirrus.Models.Network.Response.Search.GeneralSearchModules;

/// <summary>
/// Artist module in general search result.
/// </summary>
public class ArtistSearchResultModule : SearchModuleBase
{
    /// <summary>
    /// Artists matching with the keyword.
    /// </summary>
    [JsonPropertyName("artists")] public List<ArtistDetail> Artists { get; init; } = new();
    
    /// <summary>
    /// Whether there are more matched artists other than current list.
    /// </summary>
    [JsonPropertyName("more")] public bool HasMore { get; init; }
    
    /// <summary>
    /// Caption on the "More" Button.
    /// </summary>
    [JsonPropertyName("moreText")] public string? MoreButtonCaption { get; init; }
}