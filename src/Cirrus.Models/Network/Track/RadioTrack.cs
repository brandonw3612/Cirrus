using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Cirrus.Models.Network.Track;

/// <summary>
/// Track returned from user's personal radio station.
/// </summary>
[DebuggerDisplay("Title: {Title}, ID: {TrackId}")]
public class RadioTrack : TrackDetail2
{
    /// <summary>
    /// Title of the disc that current track is on.
    /// </summary>
    [JsonPropertyName("disc")] public string? DiscTitle { get; init; }
    
    /// <summary>
    /// Number of the track in the disc. 0 by default.
    /// </summary>
    [JsonPropertyName("no")] public int NumberInDisc { get; init; }
    
    /// <summary>
    /// Popularity of the track. Range from 0 to 100.
    /// </summary>
    [JsonPropertyName("popularity")] public double Popularity { get; init; }
    
    /// <summary>
    /// Information for the super quality audio of the track.
    /// </summary>
    [JsonPropertyName("sqMusic")] public TrackAudioInfo2? SuperQualityAudioInfo { get; init; }
    
    /// <summary>
    /// Information for the Hi-Res quality audio of the track.
    /// </summary>
    [JsonPropertyName("hrMusic")] public TrackAudioInfo2? HiResQualityAudioInfo { get; init; }
    
    /// <summary>
    /// Translation for the track's title.
    /// </summary>
    [JsonPropertyName("transName")] public string? TitleTranslation { get; init; }
    
    /// <summary>
    /// Special marks of the track. 
    /// </summary>
    /// <remarks>
    /// = 0x142000 | 0x140000, the track is marked as "Explicit".
    /// </remarks>
    [JsonPropertyName("mark")] public long SpecialMarks { get; init; }
    
    /// <summary>
    /// A value indicating whether the track is an original or a cover.
    /// </summary>
    /// <remarks>
    /// = 0 | 1, original; <br/>
    /// = 2, cover.
    /// </remarks>
    // TODO: Further insight needed on the difference between 0 and 1.
    [JsonPropertyName("originCoverType")] public int OriginCoverType { get; init; }
    
    /// <summary>
    /// Original track information, only when current track is a cover.
    /// </summary>
    [JsonPropertyName("originSongSimpleData")] public OriginalTrackInfo? OriginalTrack { get; init; }
    
    /// <summary>
    /// A value indicating whether the track's album is unavailable.
    /// </summary>
    /// <remarks>
    /// 0, the album is available; <br/>
    /// 1, the album is unavailable.
    /// </remarks>
    [JsonPropertyName("single")] public int AlbumUnavailability { get; init; }
    
    /// <summary>
    /// Information for the standard quality audio of the track.
    /// </summary>
    [JsonPropertyName("bMusic")] public TrackAudioInfo2? StandardQualityAudioInfo { get; init; }
    
    /// <summary>
    /// Information for the high quality audio of the track.
    /// </summary>
    [JsonPropertyName("hMusic")] public TrackAudioInfo2? HighQualityAudioInfo { get; init; }
    
    /// <summary>
    /// Information for the medium quality audio of the track.
    /// </summary>
    [JsonPropertyName("mMusic")] public TrackAudioInfo2? MediumQualityAudioInfo { get; init; }
    
    /// <summary>
    /// Information for the low quality audio of the track.
    /// </summary>
    [JsonPropertyName("lMusic")] public TrackAudioInfo2? LowQualityAudioInfo { get; init; }
    
    /// <summary>
    /// Reason that current track is recommended from the radio.
    /// </summary>
    [JsonPropertyName("reason")] public string? PickedReason { get; init; }
    
    /// <summary>
    /// Permission for the track.
    /// </summary>
    [JsonPropertyName("privilege")] public TrackAudioInfo2? Permission { get; init; }
}