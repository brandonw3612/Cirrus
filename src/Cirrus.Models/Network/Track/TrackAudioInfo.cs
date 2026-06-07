using System.Text.Json.Serialization;

namespace Cirrus.Models.Network.Track;

/// <summary>
/// Information of a track's audio, usually in certain quality.
/// </summary>
public class TrackAudioInfo
{
    /// <summary>
    /// Bit rate of the audio.
    /// </summary>
    [JsonPropertyName("br")] public int BitRate { get; init; }

    /// <summary>
    /// Size of the audio file, presented in bytes.
    /// </summary>
    [JsonPropertyName("size")] public long Size { get; init; }
}

/// <summary>
/// Information of a track's audio, usually in certain quality.
/// </summary>
public class TrackAudioInfo2
{
    /// <summary>
    /// ID of the track, defined by NetEase.
    /// </summary>
    [JsonPropertyName("id")] public ulong TrackId { get; init; }
    
    /// <summary>
    /// Size of the original audio file, in bytes.
    /// </summary>
    [JsonPropertyName("size")] public long Size { get; init; }
    
    /// <summary>
    /// Extension of the original audio file.
    /// </summary>
    [JsonPropertyName("extension")] public string? FileExtension { get; init; }
    
    /// <summary>
    /// Sample rate of the audio, in Hz.
    /// </summary>
    [JsonPropertyName("sr")] public int SampleRate { get; init; }
    
    /// <summary>
    /// Bit rate of the audio file, in bps.
    /// </summary>
    [JsonPropertyName("bitrate")] public int BitRate { get; init; }
    
    /// <summary>
    /// Total plays of the track.
    /// </summary>
    [JsonPropertyName("playTime")] public long Plays { get; init; }
    
    /// <summary>
    /// Delta of the audio's volume.
    /// </summary>
    [JsonPropertyName("volumeDelta")] public double VolumeDelta { get; init; }
}