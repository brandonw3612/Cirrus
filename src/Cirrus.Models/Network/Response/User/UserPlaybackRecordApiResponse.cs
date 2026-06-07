using System.Text.Json.Serialization;
using Cirrus.Models.Network.User;

namespace Cirrus.Models.Network.Response.User;

/// <summary>
/// Response for API user/playback-record.
/// </summary>
public class UserPlaybackRecordApiResponse : MusicApiResponse
{
    /// <summary>
    /// All tracks in the user's weekly playback record.
    /// </summary>
    [JsonPropertyName("weekData")] public List<PlaybackRecordItem> WeeklyTracks { get; init; } = new();
    
    /// <summary>
    /// All tracks in the user's all-time playback record.
    /// </summary>
    [JsonPropertyName("allData")] public List<PlaybackRecordItem> AllTimeTracks { get; init; } = new();
}

