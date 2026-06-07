using System.Diagnostics;
using System.Text.Json.Serialization;
using Cirrus.Models.Network.Track;

namespace Cirrus.Models.Network.User;

/// <summary>
/// Single record item on a user's playback record list.
/// </summary>
[DebuggerDisplay("Title: {Track.Title}, ID: {Track.TrackId}, Plays: {Plays}")]
public class PlaybackRecordItem
{
    /// <summary>
    /// Plays of the track.
    /// </summary>
    [JsonPropertyName("playCount")] public long Plays { get; init; }

    /// <summary>
    /// Relative score of plays to the most played track, presented in an integer. Range from 0 to 100.
    /// </summary>
    [JsonPropertyName("score")] public int RelativePlays { get; init; }

    /// <summary>
    /// Track of the playback record item.
    /// </summary>
    [JsonPropertyName("song")] public TrackDetail? Track { get; init; }
}