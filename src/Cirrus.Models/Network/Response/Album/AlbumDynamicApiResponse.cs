using System.Text.Json.Serialization;

namespace Cirrus.Models.Network.Response.Album;

/// <summary>
/// Response for API album/dynamic.
/// </summary>
public class AlbumDynamicApiResponse : MusicApiResponse
{
    /// <summary>
    /// Total count of shares on the current album.
    /// </summary>
    [JsonPropertyName("shareCount")] public int ShareCount { get; init; }
    
    /// <summary>
    /// Whether the current user is subscribing the album.
    /// </summary>
    [JsonPropertyName("isSub")] public bool IsSubscribing { get; init; }
    
    /// <summary>
    /// Unix timestamp that the current user subscribed the album.
    /// </summary>
    [JsonPropertyName("subTime")] public long SubscribingTimestamp { get; init; }
    
    /// <summary>
    /// Total count of the album's subscribers.
    /// </summary>
    [JsonPropertyName("subCount")] public int SubscriberCount { get; init; }
}