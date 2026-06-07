using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Cirrus.Models.Network.Track;

/// <summary>
/// Track in user's cloud drive.
/// </summary>
[DebuggerDisplay("Title: {Title}, ID: {TrackId}")]
public class CloudDriveTrack
{
    /// <summary>
    /// Detail of the track.
    /// </summary>
    [JsonPropertyName("simpleSong")] public TrackDetail? Track { get; init; }
    
    /// <summary>
    /// Album title of the track.
    /// </summary>
    [JsonPropertyName("album")] public string AlbumTitle { get; init; } = "Unknown Album";
    
    /// <summary>
    /// Artist name of the track.
    /// </summary>
    [JsonPropertyName("artist")] public string ArtistName { get; init; } = "Unknown Artist";
    
    /// <summary>
    /// Bit rate of the track, in kbps.
    /// </summary>
    [JsonPropertyName("bitrate")] public int BitRate { get; init; }
    
    /// <summary>
    /// ID of the track, defined by NetEase.
    /// </summary>
    [JsonPropertyName("songId")] public ulong TrackId { get; init; }
    
    /// <summary>
    /// Title of the track.
    /// </summary>
    [JsonPropertyName("songName")] public string Title { get; init; } = "Unknown Track";
    
    /// <summary>
    /// Timestamp that the track was uploaded to the cloud drive.
    /// </summary>
    [JsonPropertyName("addTime")] public long UploadTimestamp { get; init; }
    
    /// <summary>
    /// Version of the data.
    /// </summary>
    [JsonPropertyName("version")] public int Version { get; init; }
    
    /// <summary>
    /// Size of the original audio file.
    /// </summary>
    [JsonPropertyName("fileSize")] public long OriginalFileSize { get; init; }
    
    /// <summary>
    /// Name of the original audio file.
    /// </summary>
    [JsonPropertyName("fileName")] public string? OriginalFileName { get; init; }
}