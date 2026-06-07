using System.Text.Json.Serialization;
using Cirrus.Models.Network.Playlist;
using Cirrus.Models.Shared.Search;

namespace Cirrus.Models.Network.Response.Search;

/// <summary>
/// Response for API search/cloud-search when search target is <see cref="SearchTarget.Playlist"/>.
/// </summary>
public class PlaylistSearchApiResponse : SearchApiResponse, IJsonOnDeserialized
{
    /// <summary>
    /// Playlists matching with the keyword.
    /// </summary>
    [JsonIgnore] public List<PlaylistDetail> Playlists { get; private set; } = new();
    
    /// <summary>
    /// Total count of playlists that match with the keyword.
    /// </summary>
    [JsonIgnore] public int TotalCount { get; private set; }
    
    [JsonInclude] [JsonPropertyName("result")] internal PlaylistSearchResult? Result { get; init; }

    void IJsonOnDeserialized.OnDeserialized()
    {
        if (Result is null) return;
        Playlists = Result.Playlists ?? [];
        TotalCount = Result.TotalCount;
    }

    public class PlaylistSearchResult
    {
        [JsonPropertyName("playlists")] public List<PlaylistDetail>? Playlists { get; init; } = new();
        [JsonPropertyName("playlistCount")] public int TotalCount { get; init; }
    }
}