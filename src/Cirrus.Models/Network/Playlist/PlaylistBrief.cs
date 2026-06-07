using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Cirrus.Models.Network.Playlist;

/// <summary>
/// Brief of a playlist
/// </summary>
[DebuggerDisplay("Title: {Title}, ID: {PlaylistId}")]
public class PlaylistBrief
{
    /// <summary>
    /// ID of the playlist, defined by NetEase.
    /// </summary>
    [JsonPropertyName("id")] public long PlaylistId { get; init; }
    
    /// <summary>
    /// Type of the playlist, presented in an integer.
    /// </summary>
    /// <remarks>
    /// 5 - User like list;
    /// 10 - Top chart;
    /// 100 - Official dynamic playlist;
    /// 20 - User's annual top chart;
    /// 300 - Shared playlist;
    /// Others - Normal Playlist.
    /// </remarks>
    [JsonPropertyName("type")] public int PlaylistType { get; init; }
    
    /// <summary>
    /// Title of the playlist.
    /// </summary>
    [JsonPropertyName("name")] public string Title { get; init; } = "Unknown Playlist";
    
    /// <summary>
    /// Copywriter of the playlist.
    /// </summary>
    [JsonPropertyName("copywriter")] public string? Copywriter { get; init; }
    
    /// <summary>
    /// Url of the playlist's cover image.
    /// </summary>
    [JsonPropertyName("picUrl")] public string? CoverImageUrl { get; init; }
    
    /// <summary>
    /// Unix timestamp of the last update on the track count of the playlist.
    /// </summary>
    [JsonPropertyName("trackNumberUpdateTime")] public long TrackCountUpdateTimestamp { get; init; }
    
    /// <summary>
    /// Total plays of the playlist since created.
    /// </summary>
    [JsonPropertyName("playCount")] public long Plays { get; init; }
    
    /// <summary>
    /// Total count of tracks on the playlist.
    /// </summary>
    [JsonPropertyName("trackCount")] public int TrackCount { get; init; }
    
    /// <summary>
    /// Whether the playlist is certified by NetEase as of high quality. 
    /// </summary>
    [JsonPropertyName("highQuality")] public bool IsOfHighQuality { get; init; }
}