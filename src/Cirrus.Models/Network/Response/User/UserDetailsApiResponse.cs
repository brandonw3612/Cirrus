using System.Text.Json.Serialization;
using Cirrus.Models.Network.User;

namespace Cirrus.Models.Network.Response.User;

/// <summary>
/// Response for API user/details.
/// </summary>
public class UserDetailsApiResponse : MusicApiResponse
{
    /// <summary>
    /// Level of the user.
    /// </summary>
    [JsonPropertyName("level")] public int Level { get; init; }

    /// <summary>
    /// Total count of tracks that have been played by the user.
    /// </summary>
    [JsonPropertyName("listenSongs")] public long TrackCount { get; init; }

    /// <summary>
    /// Profile of the user.
    /// </summary>
    [JsonPropertyName("profile")] public UserProfile? Profile { get; init; }

    /// <summary>
    /// Whether the user has set the playback record as publicly accessible.
    /// </summary>
    [JsonPropertyName("peopleCanSeeMyPlayRecord")] public bool IsPlaybackRecordPublic { get; init; }

    /// <summary>
    /// Unix timestamp that the user registered the account.
    /// </summary>
    [JsonPropertyName("createTime")] public long RegisteredTimestamp { get; init; }

    /// <summary>
    /// Number of days the user has been using the account.
    /// </summary>
    [JsonPropertyName("createDays")] public long RegisteredDays { get; init; }
}