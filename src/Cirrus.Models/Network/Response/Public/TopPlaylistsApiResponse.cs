using System.Text.Json.Serialization;
using Cirrus.Models.Network.Playlist;

namespace Cirrus.Models.Network.Response.Public;

// TODO: Documentation needed.

public class TopPlaylistsApiResponse : MusicApiResponse
{
    [JsonPropertyName("playlists")] public List<PlaylistDetail> Playlists { get; init; } = new();
    [JsonPropertyName("total")] public int Total { get; init; }
    [JsonPropertyName("more")] public bool More { get; init; }
    [JsonPropertyName("cat")] public string Category { get; init; } = "全部";
}