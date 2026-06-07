using System.Text.Json.Serialization;
using Cirrus.Models.Network.Track;

namespace Cirrus.Models.Network.Response.Search.GeneralSearchModules;

/// <summary>
/// Track module in general search result.
/// </summary>
public class TrackSearchResultModule : SearchModuleBase
{
    /// <summary>
    /// Tracks matching with the keyword.
    /// </summary>
    [JsonPropertyName("songs")] public List<TrackDetail> Tracks { get; init; } = new();
    
    /// <summary>
    /// Whether there are more matched tracks other than current list.
    /// </summary>
    [JsonPropertyName("more")] public bool HasMore { get; init; }
    
    /// <summary>
    /// Caption on the "More" Button.
    /// </summary>
    [JsonPropertyName("moreText")] public string? MoreButtonCaption { get; init; }
}