using System.Text.Json.Serialization;
using Cirrus.Models.Network.Track;

namespace Cirrus.Models.Network.Response.Public;

/// <summary>
/// Response for API public/new-tracks.
/// </summary>
public class NewTracksApiResponse : MusicApiResponse
{
    /// <summary>
    /// New tracks list.
    /// </summary>
    [JsonPropertyName("data")] public List<TrackDetail3> Tracks { get; init; } = new();
}


