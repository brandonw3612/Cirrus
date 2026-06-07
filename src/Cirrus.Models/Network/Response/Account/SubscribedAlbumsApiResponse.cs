using System.Text.Json.Serialization;
using Cirrus.Models.Network.Album;

namespace Cirrus.Models.Network.Response.Account;

/// <summary>
/// Response for API account/subscribed-albums.
/// </summary>
public class SubscribedAlbumsApiResponse : MusicApiResponse
{
    /// <summary>
    /// User's subscribed albums.
    /// </summary>
    [JsonPropertyName("data")] public List<AlbumDetail2> Albums { get; init; } = new();
    
    /// <summary>
    /// Full count of the user's album subscriptions.
    /// </summary>
    [JsonPropertyName("count")] public int FullAlbumCount { get; init; }
    
    /// <summary>
    /// Whether the user has more subscribed albums after the page.
    /// </summary>
    [JsonPropertyName("hasMore")] public bool HasMore { get; init; }
}