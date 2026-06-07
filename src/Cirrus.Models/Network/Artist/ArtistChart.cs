using System.Text.Json.Serialization;

namespace Cirrus.Models.Network.Artist;

/// <summary>
/// Artist chart.
/// </summary>
public class ArtistChart
{
    /// <summary>
    /// Url of the chart's cover image.
    /// </summary>
    [JsonPropertyName("coverUrl")] public string? CoverImageUrl { get; init; }
    
    /// <summary>
    /// Title of the chart.
    /// </summary>
    [JsonPropertyName("name")] public string? Title { get; init; }
    
    /// <summary>
    /// Update frequency of the chart.
    /// </summary>
    [JsonPropertyName("updateFrequency")] public string? UpdateFrequency { get; init; }
}