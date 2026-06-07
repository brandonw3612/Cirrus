using System.Text.Json.Serialization;

namespace Cirrus.Models.Network.Response.User;

/// <summary>
/// Response for API user/like-list.
/// </summary>
public class UserLikeListApiResponse : MusicApiResponse
{
    /// <summary>
    /// IDs of the tracks on the user's like list.
    /// </summary>
    [JsonPropertyName("ids")] public List<ulong> TrackIds { get; init; } = new();
    
    /// <summary>
    /// Timestamp of the check point.
    /// </summary>
    [JsonPropertyName("checkPoint")] public long CheckPointTimestamp { get; init; }
}