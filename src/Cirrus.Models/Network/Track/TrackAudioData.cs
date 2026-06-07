using System.Text.Json.Serialization;

namespace Cirrus.Models.Network.Track;

/// <summary>
/// Audio data of a track in the specified quality.
/// </summary>
public class TrackAudioData
{
    /// <summary>
    /// ID of the track, defined by NetEase.
    /// </summary>
    [JsonPropertyName("id")] public ulong TrackId { get; init; }
    
    /// <summary>
    /// Url of the track's audio.
    /// </summary>
    [JsonPropertyName("url")] public string? AudioUrl { get; init; }
    
    /// <summary>
    /// Bit rate of the audio file, in bps.
    /// </summary>
    [JsonPropertyName("br")] public int BitRate { get; init; }
    
    /// <summary>
    /// Size of the original audio file, in bytes.
    /// </summary>
    [JsonPropertyName("size")] public long OriginalFileSize { get; init; }
    
    /// <summary>
    /// MD5 of the original audio file.
    /// </summary>
    [JsonPropertyName("md5")] public string? OriginalFileMd5 { get; init; }
    
    /// <summary>
    /// File extension of the original audio file.
    /// </summary>
    [JsonPropertyName("type")] public string? OriginalFileExtension { get; init; }
    
    /// <summary>
    /// Gain of the audio.
    /// </summary>
    [JsonPropertyName("gain")] public double Gain { get; init; }
    
    /// <summary>
    /// Charging-type of the track for different members.
    /// </summary>
    /// <remarks>
    /// = 0, available for any users; <br/>
    /// = 1, available for VIP users only; <br/>
    /// = 4, from a digital album that is sold separately; <br/>
    /// = 8, available for any users (normal quality only). 
    /// </remarks>
    [JsonPropertyName("fee")] public int ChargingType { get; init; }
    
    /// <summary>
    /// Whether the track has been paid, presented in an integer.
    /// </summary>
    [JsonPropertyName("payed")] public int IsPurchased { get; init; }
    
    /// <summary>
    /// Flag of the track, defined by NetEase.
    /// </summary>
    [JsonPropertyName("flag")] public int Flag { get; init; }
    
    /// <summary>
    /// Quality of the audio.
    /// </summary>
    [JsonPropertyName("level")] public string? AudioQuality { get; init; }
    
    /// <summary>
    /// Codec of the audio file.
    /// </summary>
    [JsonPropertyName("encodeType")] public string? Codec { get; init; }
    
    /// <summary>
    /// Duration of the track, presented in total milliseconds of the track.
    /// </summary>
    [JsonPropertyName("time")] public int DurationMilliseconds { get; init; }
} 