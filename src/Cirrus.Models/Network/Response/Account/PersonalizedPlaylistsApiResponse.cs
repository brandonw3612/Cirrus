using System.Text.Json.Serialization;
using Cirrus.Models.Network.Playlist;

namespace Cirrus.Models.Network.Response.Account;

/// <summary>
/// Response for API account/personalized-playlists.
/// </summary>
public class PersonalizedPlaylistsApiResponse : MusicApiResponse
{
    /// <summary>
    /// Personalized playlists for current user.
    /// </summary>
    [JsonPropertyName("result")] public List<PlaylistBrief> Playlists { get; init; } = new();
}