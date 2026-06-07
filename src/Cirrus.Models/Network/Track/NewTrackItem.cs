using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Cirrus.Models.Network.Track;

/// <summary>
/// Item in new track list.
/// </summary>
[DebuggerDisplay("Title: {Title}, ID: {TrackId}")]
public class NewTrackItem
{
    /// <summary>
    /// ID of the track, defined by NetEase.
    /// </summary>
    [JsonPropertyName("id")] public ulong TrackId { get; init; }
    
    /// <summary>
    /// Title of the track.
    /// </summary>
    [JsonPropertyName("name")] public string Title { get; init; } = "Unknown Track";
    
    /// <summary>
    /// Url of the album's cover image.
    /// </summary>
    [JsonPropertyName("picUrl")] public string? AlbumCoverImageUrl { get; init; }
    
    /// <summary>
    /// Track details.
    /// </summary>
    [JsonPropertyName("song")] public TrackDetail? Track { get; init; }
}