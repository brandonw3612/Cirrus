using System.Text.Json.Serialization;
using Cirrus.Models.Network.Public;

namespace Cirrus.Models.Network.Response.Public;

/// <summary>
/// Response for API public/banners
/// </summary>
public class BannersApiResponse : MusicApiResponse
{
    /// <summary>
    /// Banner list on the home page.
    /// </summary>
    [JsonPropertyName("banners")] public List<Banner> Banners { get; init; } = new();
}