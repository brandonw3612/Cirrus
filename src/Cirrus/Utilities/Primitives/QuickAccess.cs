using System.Text.Json.Serialization;
using Cirrus.Models.Business.Playlist;

namespace Cirrus.Utilities.Primitives;

public class QuickAccess
{
    [JsonPropertyName("favListId")] public ulong? FavListId { get; set; } = null;
    [JsonPropertyName("playlists")] public QuickAccessPlaylist[] Playlists { get; set; } = [];
}