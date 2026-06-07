using System.Text.Json.Serialization;
using Cirrus.Models.Network.Response.Search.GeneralSearchModules;
using Cirrus.Models.Shared.Search;

namespace Cirrus.Models.Network.Response.Search;

/// <summary>
/// Response for API search/search when search target is <see cref="SearchTarget.General"/>.
/// </summary>
public class GeneralSearchApiResponse : SearchApiResponse, IJsonOnDeserialized
{
    /// <summary>
    /// Modules in the search result, ordered as NetEase recommended.
    /// </summary>
    [JsonIgnore] public List<SearchModuleBase> Modules { get; private set; } = new();
    
    [JsonInclude] [JsonPropertyName("result")] internal GeneralSearchResult? Result { get; init; }

    void IJsonOnDeserialized.OnDeserialized()
    {
        if (Result is null) return;
        Modules = Result.ModulesOrder.Select<string, SearchModuleBase?>(module => module switch
        {
            "song" => Result.TrackModule,
            "playList" => Result.PlaylistModule,
            "artist" => Result.ArtistModule,
            "album" => Result.AlbumModule,
            "sim_query" => Result.SimilarQuerySuggestionsModule,
            "user" => Result.UserModule,
            _ => null
        }).OfType<SearchModuleBase>().ToList();
    }
    
    public class GeneralSearchResult
    {
        [JsonPropertyName("song")] public TrackSearchResultModule? TrackModule { get; init; }
        [JsonPropertyName("playList")] public PlaylistSearchResultModule? PlaylistModule { get; init; }
        [JsonPropertyName("artist")] public ArtistSearchResultModule? ArtistModule { get; init; }
        [JsonPropertyName("album")] public AlbumSearchResultModule? AlbumModule { get; init; }
        [JsonPropertyName("sim_query")] public SimilarQuerySuggestionsModule? SimilarQuerySuggestionsModule { get; init; }
        [JsonPropertyName("user")] public UserSearchResultModule? UserModule { get; init; }
        [JsonPropertyName("order")] public List<string> ModulesOrder { get; init; } = new();
    }
}