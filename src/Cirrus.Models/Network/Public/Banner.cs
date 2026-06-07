using System.Text.Json.Serialization;

namespace Cirrus.Models.Network.Public;

/// <summary>
/// Banner item on Home page.
/// </summary>
public class Banner
{
    /// <summary>
    /// Url of the banner's image.
    /// </summary>
    [JsonPropertyName("imageUrl")] public string? BannerImageUrl { get; init; }
    
    /// <summary>
    /// ID of the targeted resource.
    /// </summary>
    [JsonPropertyName("targetId")] public ulong TargetId { get; init; }
    
    /// <summary>
    /// Type of the targeted resource.
    /// </summary>
    // TODO: Classification / Documentation needed.
    [JsonPropertyName("targetType")] public int TargetType { get; init; }
    
    /// <summary>
    /// String-typed banner type title color.
    /// </summary>
    [JsonPropertyName("titleColor")] public string? TitleColorString { get; init; }
    
    /// <summary>
    /// Title of the banner type.
    /// </summary>
    [JsonPropertyName("typeTitle")] public string? TypeTitle { get; init; }
    
    /// <summary>
    /// Url of the resource, usually considered as fallback.
    /// </summary>
    [JsonPropertyName("url")] public string? TargetUrl { get; init; }
    
    /// <summary>
    /// Whether the resource is exclusive on NetEase.
    /// </summary>
    [JsonPropertyName("exclusive")] public bool IsExclusive { get; init; }
}