using System.Text.Json.Serialization;
using Cirrus.Models.Network.Permission;
using Cirrus.Models.Network.Track;

namespace Cirrus.Models.Network.Response.Track;

/// <summary>
/// Response for API track/details.
/// </summary>
public class TrackDetailsApiResponse : MusicApiResponse, IJsonOnDeserialized
{
    /// <summary>
    /// Detail of the requested tracks.
    /// </summary>
    [JsonPropertyName("songs")] public List<TrackDetail> Tracks { get; init; } = new();

    [JsonInclude] [JsonPropertyName("privileges")]
    internal List<TrackPermission> TrackPermissions { get; init; } = new();

    void IJsonOnDeserialized.OnDeserialized()
    {
        for (var i = 0; i < TrackPermissions.Count && i < Tracks.Count; i++)
        {
            Tracks[i].Permission = TrackPermissions[i];
        }
    }
}