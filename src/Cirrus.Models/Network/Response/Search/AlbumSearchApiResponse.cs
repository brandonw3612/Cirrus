using System.Text.Json.Serialization;
using Cirrus.Models.Network.Album;
using Cirrus.Models.Shared.Search;

namespace Cirrus.Models.Network.Response.Search;

/// <summary>
/// Response for API search/cloud-search when search target is <see cref="SearchTarget.Album"/>.
/// </summary>
public class AlbumSearchApiResponse : SearchApiResponse, IJsonOnDeserialized
{
    /// <summary>
    /// Albums matching with the keyword.
    /// </summary>
    [JsonIgnore] public List<AlbumDetail> Albums { get; private set; } = new();
    
    /// <summary>
    /// Total count of albums that match with the keyword.
    /// </summary>
    [JsonIgnore] public int TotalCount { get; private set; }
    
    [JsonInclude] [JsonPropertyName("result")] internal AlbumSearchResult? Result { get; init; }
    
    void IJsonOnDeserialized.OnDeserialized()
    {
        if (Result is null) return;
        Albums = Result.Albums ?? [];
        TotalCount = Result.TotalCount;
    }

    public class AlbumSearchResult
    {
        [JsonPropertyName("albums")] public List<AlbumDetail>? Albums { get; init; } = new();
        [JsonPropertyName("albumCount")] public int TotalCount { get; init; }
    }
}