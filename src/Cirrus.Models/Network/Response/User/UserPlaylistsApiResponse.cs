using System.Text.Json.Serialization;
using Cirrus.Models.Network.Playlist;

namespace Cirrus.Models.Network.Response.User;

/// <summary>
/// Response for API user/playlists.
/// </summary>
public class UserPlaylistsApiResponse : MusicApiResponse
{
    /// <summary>
    /// Whether the user has more playlists after the page.
    /// </summary>
    [JsonPropertyName("more")] public bool HasMore { get; init; }
    
    /// <summary>
    /// Playlists of the user.
    /// </summary>
    [JsonPropertyName("playlist")] public List<PlaylistDetail> Playlists { get; init; } = new();
}