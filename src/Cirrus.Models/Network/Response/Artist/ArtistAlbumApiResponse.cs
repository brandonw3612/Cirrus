using System.Text.Json.Serialization;
using Cirrus.Models.Network.Album;
using Cirrus.Models.Network.Artist;

namespace Cirrus.Models.Network.Response.Artist;

/// <summary>
/// Response for API artist/albums.
/// </summary>
public class ArtistAlbumApiResponse : MusicApiResponse
{
    /// <summary>
    /// Artist of the query.
    /// </summary>
    [JsonPropertyName("artist")] public ArtistDetail? Artist { get; init; }
    
    /// <summary>
    /// Artist's albums.
    /// </summary>
    [JsonPropertyName("hotAlbums")] public List<AlbumDetail> Albums { get; init; } = new();
    
    /// <summary>
    /// Whether the artist has more albums after the page.
    /// </summary>
    [JsonPropertyName("more")] public bool HasMore { get; init; }
}
