using System.Text.Json.Serialization;

namespace Cirrus.Models.Network.User;

/// <summary>
/// Status for a specific membership category.
/// </summary>
public class MembershipStatus
{
    /// <summary>
    /// Category code of the membership.
    /// </summary>
    [JsonPropertyName("vipCode")] public int MembershipCategoryCode { get; init; }
    
    /// <summary>
    /// Timestamp of the membership expiration.
    /// </summary>
    [JsonPropertyName("expireTime")] public long ExpireTimestamp { get; init; }
    
    /// <summary>
    /// Current membership level.
    /// </summary>
    [JsonPropertyName("vipLevel")] public int MembershipLevel { get; init; }

    /// <summary>
    /// Whether the membership is expired.
    /// </summary>
    [JsonIgnore] public bool IsExpired => DateTimeOffset.FromUnixTimeMilliseconds(ExpireTimestamp) < DateTimeOffset.Now;
}