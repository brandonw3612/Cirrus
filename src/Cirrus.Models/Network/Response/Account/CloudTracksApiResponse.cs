using System.Text.Json.Serialization;
using Cirrus.Models.Network.Track;

namespace Cirrus.Models.Network.Response.Account;

/// <summary>
/// Response for API account/cloud-tracks.
/// </summary>
public class CloudTracksApiResponse : MusicApiResponse
{
    /// <summary>
    /// Tracks in the user's cloud drive.
    /// </summary>
    [JsonPropertyName("data")] public List<CloudDriveTrack> Tracks { get; init; } = new();
    
    /// <summary>
    /// Full count of tracks in the user's cloud drive.
    /// </summary>
    [JsonPropertyName("count")] public int FullTrackCount { get; init; }
    
    /// <summary>
    /// Whether there are more tracks after the page.
    /// </summary>
    [JsonPropertyName("hasMore")] public bool HasMore { get; init; }
}