using System.Text.Json.Serialization;
using Cirrus.Models.Network.Artist;
using Cirrus.Models.Shared.Search;

namespace Cirrus.Models.Network.Response.Search;

/// <summary>
/// Response for API search/cloud-search when search target is <see cref="SearchTarget.Artist"/>.
/// </summary>
public class ArtistSearchApiResponse : SearchApiResponse, IJsonOnDeserialized
{
    /// <summary>
    /// Artists matched with the keyword.
    /// </summary>
    [JsonIgnore] public List<ArtistDetail> Artists { get; private set; } = new();

    /// <summary>
    /// Total count of artists that match with the keyword.
    /// </summary>
    [JsonIgnore] public int TotalCount { get; private set; }
    
    [JsonInclude] [JsonPropertyName("result")] internal ArtistSearchResult? Result { get; init; }

    void IJsonOnDeserialized.OnDeserialized()
    {
        if (Result is null) return;
        Artists = Result.Artists ?? [];
        TotalCount = Result.TotalCount;
    }

    public class ArtistSearchResult
    {
        [JsonPropertyName("artists")] public List<ArtistDetail>? Artists { get; init; } = new();
        [JsonPropertyName("artistCount")] public int TotalCount { get; init; }
    }
}