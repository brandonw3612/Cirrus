using System.Text.Json.Serialization;
using Cirrus.Models.Network.Album;
using Cirrus.Models.Network.Track;

namespace Cirrus.Models.Network.Response.Album;

/// <summary>
/// Response for API album/details.
/// </summary>
public class AlbumDetailsApiResponse : MusicApiResponse
{
    /// <summary>
    /// Tracks on the album.
    /// </summary>
    [JsonPropertyName("songs")] public List<TrackDetail> Tracks { get; init; } = new();
    
    /// <summary>
    /// Album detail information.
    /// </summary>
    [JsonPropertyName("album")] public AlbumDetail? Album { get; init; }
}