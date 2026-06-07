using System.Text.Json.Serialization;
using Cirrus.Models.Network.Album;

namespace Cirrus.Models.Network.Response.Search.GeneralSearchModules;

/// <summary>
/// Album module in general search result.
/// </summary>
public class AlbumSearchResultModule : SearchModuleBase
{
    /// <summary>
    /// Albums matching with the keyword.
    /// </summary>
    [JsonPropertyName("albums")] public List<AlbumDetail> Albums { get; init; } = new();
    
    /// <summary>
    /// Whether there are more matched albums other than current list.
    /// </summary>
    [JsonPropertyName("more")] public bool HasMore { get; init; }
    
    /// <summary>
    /// Caption on the "More" Button.
    /// </summary>
    [JsonPropertyName("moreText")] public string? MoreButtonCaption { get; init; }
}