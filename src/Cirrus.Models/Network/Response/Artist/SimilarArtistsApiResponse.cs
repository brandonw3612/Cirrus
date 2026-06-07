using System.Text.Json.Serialization;
using Cirrus.Models.Network.Artist;

namespace Cirrus.Models.Network.Response.Artist;

/// <summary>
/// Response for API artist/similar.
/// </summary>
public class SimilarArtistsApiResponse : MusicApiResponse
{
    [JsonPropertyName("artists")] public List<ArtistDetail> Artists { get; init; } = new();
}