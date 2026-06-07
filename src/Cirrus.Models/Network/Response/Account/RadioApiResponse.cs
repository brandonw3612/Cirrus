using System.Text.Json.Serialization;
using Cirrus.Models.Network.Track;

namespace Cirrus.Models.Network.Response.Account;

/// <summary>
/// Response for API account/radio.
/// </summary>
public class RadioApiResponse : MusicApiResponse
{
    /// <summary>
    /// Tracks on the current user's personal radio.
    /// </summary>
    [JsonPropertyName("data")] public List<RadioTrack> Tracks { get; init; } = new();
}