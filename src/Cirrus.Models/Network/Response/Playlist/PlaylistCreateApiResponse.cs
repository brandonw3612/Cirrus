using System.Text.Json.Serialization;
using Cirrus.Models.Network.Playlist;

namespace Cirrus.Models.Network.Response.Playlist;

/// <summary>
/// Response for API playlist/create.
/// </summary>
public class PlaylistCreateApiResponse : MusicApiResponse
{
    /// <summary>
    /// Detail of the created playlist.
    /// </summary>
    [JsonPropertyName("playlist")] public PlaylistDetail? Playlist { get; init; }
   
    /// <summary>
    /// ID of the created playlist.
    /// </summary>
    [JsonPropertyName("id")] public ulong PlaylistId { get; init; }
}