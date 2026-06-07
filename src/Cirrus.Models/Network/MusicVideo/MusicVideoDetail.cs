using System.Diagnostics;
using System.Text.Json.Serialization;
using Cirrus.Models.Network.Artist;

namespace Cirrus.Models.Network.MusicVideo;

/// <summary>
/// Detail of a music video.
/// </summary>
[DebuggerDisplay("Title: {Title}, ID: {MusicVideoId}")]
public class MusicVideoDetail
{
    /// <summary>
    /// ID of the music video, defined by NetEase.
    /// </summary>
    [JsonPropertyName("id")] public ulong MusicVideoId { get; init; }
    
    /// <summary>
    /// Url of the music video's cover image.
    /// </summary>
    [JsonPropertyName("cover")] public string? CoverImageUrl { get; init; }
    
    /// <summary>
    /// Title of the music video.
    /// </summary>
    [JsonPropertyName("name")] public string Title { get; init; } = "Unknown Music Video";
    
    /// <summary>
    /// Total plays of the music video.
    /// </summary>
    [JsonPropertyName("playCount")] public long Plays { get; init; }
    
    /// <summary>
    /// Brief description of the music video.
    /// </summary>
    [JsonPropertyName("briefDesc")] public string? BriefDescription { get; init; }
    
    /// <summary>
    /// Full description of the music video.
    /// </summary>
    [JsonPropertyName("desc")] public string? Description { get; init; }
    
    /// <summary>
    /// Duration of the music video, presented in total milliseconds.
    /// </summary>
    [JsonPropertyName("duration")] public long DurationMilliseconds { get; init; }
    
    /// <summary>
    /// Special marks of the music video.
    /// </summary>
    // TODO: Classification / Documentation needed.
    [JsonPropertyName("mark")] public int SpecialMarks { get; init; }
    
    /// <summary>
    /// Artists of the music video.
    /// </summary>
    [JsonPropertyName("artists")] public List<MusicVideoArtist> Artists { get; init; } = new();
    
    /// <summary>
    /// Translations for the music video's title.
    /// </summary>
    [JsonPropertyName("transNames")] public List<string> TitleTranslations { get; init; } = new();
    
    /// <summary>
    /// Title alias of the music video.
    /// </summary>
    [JsonPropertyName("alias")] public List<string> Alias { get; init; } = new();
}