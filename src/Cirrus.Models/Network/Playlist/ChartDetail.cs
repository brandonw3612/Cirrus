using System.Text.Json.Serialization;

namespace Cirrus.Models.Network.Playlist;

/// <summary>
/// Detail of a chart playlist.
/// </summary>
public class ChartDetail : PlaylistDetail
{
    /// <summary>
    /// Text displayed on the cover.
    /// </summary>
    [JsonPropertyName("coverText")] public string? CoverText { get; init; }
    
    /// <summary>
    /// Url of the playlist's icon image.
    /// </summary>
    [JsonPropertyName("iconImageUrl")] public string? IconImageUrl { get; init; }
    
    /// <summary>
    /// Chart type given by NetEase.
    /// </summary>
    [JsonPropertyName("ToplistType")] public string? ChartType { get; init; }
}