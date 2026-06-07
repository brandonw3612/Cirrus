using System.Diagnostics;
using System.Text.Json.Serialization;
using Cirrus.Models.Network.User;

namespace Cirrus.Models.Network.Podcast;

/// <summary>
/// Detail of a podcast channel.
/// </summary>
[DebuggerDisplay("Name: {Name}, ID: {PodcastChannelId}")]
public class PodcastChannelDetail
{
    /// <summary>
    /// ID of the podcast channel, defined by NetEase.
    /// </summary>
    [JsonPropertyName("id")] public ulong PodcastChannelId { get; init; }
    
    /// <summary>
    /// Creator of the podcast channel.
    /// </summary>
    [JsonPropertyName("dj")] public UserProfile? Creator { get; init; }
    
    /// <summary>
    /// Name of the podcast channel.
    /// </summary>
    [JsonPropertyName("name")] public string Name { get; init; } = "Unknown Channel";
    
    /// <summary>
    /// Url of the podcast channel's cover image.
    /// </summary>
    [JsonPropertyName("picUrl")] public string? CoverImageUrl { get; init; }
    
    /// <summary>
    /// Description of the podcast channel.
    /// </summary>
    [JsonPropertyName("desc")] public string? Description { get; init; }
    
    /// <summary>
    /// Total count of the podcast channel's subscribers.
    /// </summary>
    [JsonPropertyName("subCount")] public int SubscriberCount { get; init; }
    
    /// <summary>
    /// Total count of episodes in the podcast channel.
    /// </summary>
    [JsonPropertyName("programCount")] public int EpisodeCount { get; init; }
    
    /// <summary>
    /// Unix timestamp that the current podcast channel was created.
    /// </summary>
    [JsonPropertyName("createTime")] public long CreatedTimestamp { get; init; }
    
    /// <summary>
    /// ID of the podcast channel's category, defined by NetEase.
    /// </summary>
    [JsonPropertyName("categoryId")] public ulong CategoryId { get; init; }
    
    /// <summary>
    /// Category name of the current podcast channel.
    /// </summary>
    [JsonPropertyName("category")] public string? Category { get; init; }
    
    /// <summary>
    /// ID of the podcast channel's subcategory, defined by NetEase.
    /// </summary>
    [JsonPropertyName("secondCategoryId")] public ulong SubcategoryId { get; init; }
    
    /// <summary>
    /// Subcategory name of the current podcast channel.
    /// </summary>
    [JsonPropertyName("secondCategory")] public string? Subcategory { get; init; }
    
    /// <summary>
    /// Charging type of the podcast channel.
    /// </summary>
    // TODO: Classification / Documentation needed.
    [JsonPropertyName("radioFeeType")] public int ChargingType { get; init; }
    
    /// <summary>
    /// Whether the current user has purchased the current podcast channel.
    /// </summary>
    [JsonPropertyName("buyed")] public bool IsPurchased { get; init; }
    
    /// <summary>
    /// Total count of users that have purchased the current podcast channel.
    /// </summary>
    [JsonPropertyName("purchaseCount")] public int Purchases { get; init; }
    
    /// <summary>
    /// Unix timestamp that the latest episode in the current podcast channel was created. 
    /// </summary>
    [JsonPropertyName("lastProgramCreateTime")] public long LatestEpisodeCreatedTimestamp { get; init; }
    
    /// <summary>
    /// Name of the latest episode in the current podcast channel.
    /// </summary>
    [JsonPropertyName("lastProgramName")] public string? LatestEpisodeTitle { get; init; }
    
    /// <summary>
    /// ID of the latest episode in the current podcast channel.
    /// </summary>
    [JsonPropertyName("lastProgramId")] public ulong LatestEpisodeId { get; init; }
    
    /// <summary>
    /// Reason that the current podcast channel has been recommended.
    /// </summary>
    [JsonPropertyName("rcmdText")] public string? RecommendedReason { get; init; }
    
    /// <summary>
    /// Whether the current podcast channel has been certified as of high quality by NetEase.
    /// </summary>
    [JsonPropertyName("hightQuality")] public bool IsOfHighQuality { get; init; }
    
    /// <summary>
    /// Total plays of the current podcast channel.
    /// </summary>
    [JsonPropertyName("playCount")] public long Plays { get; init; }
    
    /// <summary>
    /// Whether the current podcast channel is private.
    /// </summary>
    [JsonPropertyName("privacy")] public bool IsPrivate { get; init; }
    
    /// <summary>
    /// Total count of user shares on the current podcast channel.
    /// </summary>
    [JsonPropertyName("shareCount")] public int Shares { get; init; }
}