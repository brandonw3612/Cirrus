using System.Text.Json.Serialization;
using Cirrus.Models.Network.Artist;
using Cirrus.Models.Network.Playlist;

namespace Cirrus.Models.Network.Response.Public;

/// <summary>
/// Response for API public/charts.
/// </summary>
public class ChartsApiResponse : MusicApiResponse
{
    /// <summary>
    /// Available chart playlists.
    /// </summary>
    [JsonPropertyName("list")] public List<ChartDetail> Charts { get; init; } = new();
    
    /// <summary>
    /// Artist chart.
    /// </summary>
    [JsonPropertyName("artistToplist")] public ArtistChart? ArtistChart { get; init; }
}