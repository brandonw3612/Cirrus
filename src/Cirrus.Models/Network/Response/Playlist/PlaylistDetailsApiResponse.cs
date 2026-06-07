using System.Text.Json.Serialization;
using Cirrus.Models.Network.Permission;
using Cirrus.Models.Network.Playlist;

namespace Cirrus.Models.Network.Response.Playlist;

/// <summary>
/// Response for API playlist/details.
/// </summary>
public class PlaylistDetailsApiResponse : MusicApiResponse, IJsonOnDeserialized
{
    /// <summary>
    /// Details of the requested playlist.
    /// </summary>
    [JsonPropertyName("playlist")] public PlaylistDetail? Playlist { get; init; }

    [JsonInclude] [JsonPropertyName("privileges")]
    internal List<TrackPermission> TrackPermissions { get; set; } = new();

#pragma warning disable CS0618 // Type or member is obsolete
    void IJsonOnDeserialized.OnDeserialized()
    {
        if (Playlist is null) return;
        for (var i = 0; i < TrackPermissions.Count && i < Playlist.Tracks.Count; i++)
        {
            Playlist.Tracks[i].Permission = TrackPermissions[i];
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}

