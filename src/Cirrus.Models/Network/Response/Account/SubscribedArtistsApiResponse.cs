using System.Text.Json.Serialization;
using Cirrus.Models.Network.Artist;

namespace Cirrus.Models.Network.Response.Account;

/// <summary>
/// Response for API account/subscribed-artists.
/// </summary>
public class SubscribedArtistsApiResponse : MusicApiResponse
{
    /// <summary>
    /// User's subscribed artists.
    /// </summary>
    [JsonPropertyName("data")] public List<ArtistDetail> Artists { get; init; } = new();
    
    /// <summary>
    /// Whether the user has more subscribed artists after the page.
    /// </summary>
    [JsonPropertyName("hasMore")] public bool HasMore { get; init; }
    
    /// <summary>
    /// Full count of the user's artist subscriptions.
    /// </summary>
    [JsonPropertyName("count")] public int FullArtistCount { get; init; }
}