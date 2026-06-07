using System.Text.Json.Serialization;
using Cirrus.Models.Network.Lyrics;
using Cirrus.Models.Network.User;

namespace Cirrus.Models.Network.Response.Track;

/// <summary>
/// Response for API track/lyrics.
/// </summary>
public class LyricsApiResponse : MusicApiResponse
{
    /// <summary>
    /// Contributor of the lyrics' translation.
    /// </summary>
    [JsonPropertyName("transUser")] public LyricsContributor? TranslationContributor { get; init; }
    
    /// <summary>
    /// Contributor of the lyrics' transcription.
    /// </summary>
    [JsonPropertyName("lyricUser")] public LyricsContributor? SyncedLyricsContributor { get; init; }
    
    /// <summary>
    /// Original lyrics of the track.
    /// </summary>
    [JsonPropertyName("lrc")] public VersionedLyrics? OriginalLyrics { get; init; }
    
    /// <summary>
    /// Karaoke lyrics of the track. Usually obsolete.
    /// </summary>
    [JsonPropertyName("klyric")] public VersionedLyrics? KaraokeLyrics { get; init; }
    
    /// <summary>
    /// Translated lyrics of the track.
    /// </summary>
    [JsonPropertyName("tlyric")] public VersionedLyrics? TranslatedLyrics { get; init; }
    
    /// <summary>
    /// Romaji lyrics of the track.
    /// </summary>
    [JsonPropertyName("romalrc")] public VersionedLyrics? RomajiLyrics { get; init; }
    
    /// <summary>
    /// Verbatim lyrics of the track.
    /// </summary>
    [JsonPropertyName("yrc")] public VersionedLyrics? B2BLyrics { get; init; }
    
    /// <summary>
    /// Verbatim translated lyrics of the track.
    /// </summary>
    [JsonPropertyName("ytlrc")] public VersionedLyrics? B2BTranslatedLyrics { get; init; }
}

