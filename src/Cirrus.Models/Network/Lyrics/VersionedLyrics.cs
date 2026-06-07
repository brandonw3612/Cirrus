using System.Text.Json.Serialization;

namespace Cirrus.Models.Network.Lyrics;

/// <summary>
/// Versioned lyrics with a version number.
/// </summary>
public class VersionedLyrics
{
    /// <summary>
    /// Version of the lyrics.
    /// </summary>
    [JsonPropertyName("version")] public int Version { get; init; }
    
    /// <summary>
    /// Lyric lines.
    /// </summary>
    [JsonPropertyName("lyric")] public string Content { get; init; } = string.Empty;
}