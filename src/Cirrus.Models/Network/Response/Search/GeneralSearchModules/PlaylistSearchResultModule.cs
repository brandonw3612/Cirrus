using System.Text.Json.Serialization;
using Cirrus.Models.Network.Playlist;

namespace Cirrus.Models.Network.Response.Search.GeneralSearchModules;

/// <summary>
/// Playlist module in general search result.
/// </summary>
public class PlaylistSearchResultModule : SearchModuleBase
{
    /// <summary>
    /// Playlists matching with the keyword.
    /// </summary>
    [JsonPropertyName("playLists")] public List<PlaylistDetail> Playlists { get; init; } = new();
    
    /// <summary>
    /// Whether there are more matched playlists other than current list.
    /// </summary>
    [JsonPropertyName("more")] public bool HasMore { get; init; }
    
    /// <summary>
    /// Caption on the "More" Button.
    /// </summary>
    [JsonPropertyName("moreText")] public string? MoreButtonCaption { get; init; }
}