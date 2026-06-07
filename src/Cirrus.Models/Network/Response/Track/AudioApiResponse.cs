using System.Text.Json.Serialization;
using Cirrus.Models.Network.Track;

namespace Cirrus.Models.Network.Response.Track;

/// <summary>
/// Response for API track/audio.
/// </summary>
public class AudioApiResponse : MusicApiResponse
{
    /// <summary>
    /// Audio data in the response.
    /// </summary>
    [JsonPropertyName("data")] public List<TrackAudioData> Audios { get; init; } = new();
}