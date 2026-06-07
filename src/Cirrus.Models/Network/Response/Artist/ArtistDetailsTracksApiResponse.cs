using System.Text.Json.Serialization;
using Cirrus.Models.Network.Artist;
using Cirrus.Models.Network.Track;

namespace Cirrus.Models.Network.Response.Artist;

/// <summary>
/// Response for API artist/tracks.
/// </summary>
public class ArtistDetailsTracksApiResponse : MusicApiResponse
{
    /// <summary>
    /// Detail of the artist.
    /// </summary>
    [JsonPropertyName("artist")] public ArtistDetail? Artist { get; init; }

    /// <summary>
    /// Hot tracks of the artists.
    /// </summary>
    [JsonPropertyName("hotSongs")] public List<TrackDetail> Tracks { get; init; } = new();
    
    /// <summary>
    /// Whether the artist has more tracks.
    /// </summary>
    [JsonPropertyName("more")] public bool HasMore { get; init; }
}