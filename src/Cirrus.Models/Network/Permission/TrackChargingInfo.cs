using System.Text.Json.Serialization;

namespace Cirrus.Models.Network.Permission;

/// <summary>
/// Charging info for the specific quality of a track.
/// </summary>
public class TrackChargingInfo
{
    /// <summary>
    /// Bit rate of the track's current quality.
    /// </summary>
    [JsonPropertyName("rate")] public int BitRate { get; init; }
    
    /// <summary>
    /// Charging-type of the track's current quality.
    /// </summary>
    /// <remarks>
    /// 0, available for all users; <br/>
    /// 1, VIP users only.
    /// </remarks>
    [JsonPropertyName("chargeType")] public int ChargingType { get; init; }
}