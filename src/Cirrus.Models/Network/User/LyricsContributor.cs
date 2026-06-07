using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Cirrus.Models.Network.User;

/// <summary>
/// Contributor information for lyrics and translations.
/// </summary>
[DebuggerDisplay("Nickname: {Nickname}, ID: {UserId}")]
public class LyricsContributor
{
    /// <summary>
    /// ID of the contribution.
    /// </summary>
    [JsonPropertyName("id")] public ulong ContributionId { get; init; }
    
    /// <summary>
    /// Status of the contribution.
    /// </summary>
    [JsonPropertyName("status")] public int ContributionStatus { get; init; }
    
    /// <summary>
    /// Whether the contribution was on demand.
    /// </summary>
    [JsonPropertyName("demand")] public int ContributionDemand { get; init; }
    
    /// <summary>
    /// ID of the contributor.
    /// </summary>
    [JsonPropertyName("userid")] public ulong UserId { get; init; }
    
    /// <summary>
    /// Nickname of the contributor.
    /// </summary>
    [JsonPropertyName("nickname")] public string Nickname { get; init; } = "Unknown User";
    
    /// <summary>
    /// Unix timestamp of the contribution.
    /// </summary>
    [JsonPropertyName("uptime")] public long UploadTimestamp { get; init; }
}