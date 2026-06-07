using System.Text.Json.Serialization;

namespace Cirrus.Models.Network.Track;

/// <summary>
/// Track match result for lyrics-targeted search. Containing basic information and additional lyrics match result.
/// </summary>
public class LyricsMatchTrack : TrackDetail
{
    /// <summary>
    /// Lyrics that match with the keyword. The first line contains the matching word.
    /// </summary>
    [JsonPropertyName("lyrics")] public List<string> Lyrics { get; init; } = new();
}