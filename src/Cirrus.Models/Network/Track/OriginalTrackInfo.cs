using System.Diagnostics;
using System.Text.Json.Serialization;
using Cirrus.Models.Shared.Album;
using Cirrus.Models.Shared.Artist;

namespace Cirrus.Models.Network.Track;

/// <summary>
/// Original track information matched with a cover track.
/// </summary>
[DebuggerDisplay("Title: {Title}, ID: {TrackId}")]
public class OriginalTrackInfo
{
    /// <summary>
    /// ID of the original track.
    /// </summary>
    [JsonPropertyName("songId")] public ulong TrackId { get; init; }

    /// <summary>
    /// Title of the original track.
    /// </summary>
    [JsonPropertyName("name")] public string Title { get; init; } = "Unknown Track";

    /// <summary>
    /// Artists of the original track.
    /// </summary>
    [JsonPropertyName("artists")] public List<TrackArtist> Artists { get; init; } = new();
    
    /// <summary>
    /// Album of the original track.
    /// </summary>
    [JsonPropertyName("albumMeta")] public TrackAlbum? Album { get; init; }
}