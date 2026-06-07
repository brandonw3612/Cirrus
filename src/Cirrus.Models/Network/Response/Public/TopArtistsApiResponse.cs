using System.Text.Json.Serialization;
using Cirrus.Models.Network.Artist;

namespace Cirrus.Models.Network.Response.Public;

// TODO: Documentation needed.

public class TopArtistsApiResponse : MusicApiResponse
{
    [JsonPropertyName("artists")] public List<ArtistDetail> Artists { get; init; } = new();
    [JsonPropertyName("more")] public bool HasMore { get; init; }
}