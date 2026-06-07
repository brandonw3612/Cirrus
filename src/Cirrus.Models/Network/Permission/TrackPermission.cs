using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Cirrus.Models.Network.Permission;

/// <summary>
/// User permission on certain track.
/// </summary>
[DebuggerDisplay("ID: {TrackId}")]
public class TrackPermission
{
    /// <summary>
    /// ID of the track.
    /// </summary>
    [JsonPropertyName("id")] public ulong TrackId { get; init; }
    
    /// <summary>
    /// Charging type of the track for different members.
    /// </summary>
    /// <remarks>
    /// = 0, available for any users; <br/>
    /// = 1, available for VIP users only; <br/>
    /// = 4, from a digital album that is sold separately; <br/>
    /// = 8, available for any users (normal quality only). 
    /// </remarks>
    [JsonPropertyName("fee")] public int ChargingType { get; init; }
    
    /// <summary>
    /// A value indicating whether the user has purchased the track.
    /// </summary>
    /// <remarks>
    /// = 3 | 5, the track is already purchased.
    /// </remarks>
    [JsonPropertyName("payed")] public int PurchaseStatus { get; init; }
    
    /// <summary>
    /// A value indicating whether the track is available in the library.
    /// </summary>
    /// <remarks>
    /// = -200, the track is unavailable.
    /// </remarks>
    [JsonPropertyName("st")] public int TrackAvailability { get; init; }
    
    /// <summary>
    /// Whether the track is uploaded to current user's music cloud drive.
    /// </summary>
    [JsonPropertyName("cs")] public bool IsFromCloudDrive { get; init; }
    
    /// <summary>
    /// Best quality of the track, in bit rate.
    /// </summary>
    [JsonPropertyName("maxbr")] public int MaxBitRate { get; init; }
    
    /// <summary>
    /// Best quality of the track available for playback, in bit rate.
    /// </summary>
    [JsonPropertyName("playMaxbr")] public int MaxBitRatePlayback { get; init; }
    
    /// <summary>
    /// Best quality of the track available for download, in bit rate.
    /// </summary>
    [JsonPropertyName("downloadMaxbr")] public int MaxBitRateDownload { get; init; }

    /// <summary>
    /// Charging info list for different quality of the track.
    /// </summary>
    [JsonPropertyName("chargeInfoList")] public List<TrackChargingInfo> ChargingInfos { get; init; } = new();
}