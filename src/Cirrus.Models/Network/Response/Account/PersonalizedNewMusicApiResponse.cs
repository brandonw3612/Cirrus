using System.Text.Json.Serialization;
using Cirrus.Models.Network.Track;

namespace Cirrus.Models.Network.Response.Account;

/// <summary>
/// Response for API account/personalized-new-music.
/// </summary>
public class PersonalizedNewMusicApiResponse : MusicApiResponse
{
    /// <summary>
    /// New tracks recommendations.
    /// </summary>
    [JsonPropertyName("result")] public List<TrackDetail> Tracks { get; init; } = new();
    
    /// <summary>
    /// Category code.
    /// </summary>
    [JsonPropertyName("category")] public int Category { get; init; } = -1;
}