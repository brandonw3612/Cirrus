using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Search;
using Cirrus.Models.Shared.Search;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;
using SearchApiParameter = (string Keyword, Cirrus.Models.Shared.Search.SearchTarget SearchTarget, (int? Offset, int? Limit) Paging);

namespace Cirrus.Network.Api;

partial class Search
{
    /// <summary>
    /// Searches for general music data.
    /// API Route: /api/search/search.
    /// </summary>
    [MusicApi("search")]
    internal MusicApi<SearchApiParameter, SearchApiResponse> SearchApi => field ??= new(
        $"{RouteBase}/search",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.RequestBase}/weapi/search/get",
        p => new()
        {
            ["s"] = p.Keyword,
            ["type"] = (int) p.SearchTarget,
            ["limit"] = p.Paging.Limit ?? 30,
            ["offset"] = p.Paging.Offset ?? 0,
            ["total"] = true
        });

    /// <summary>
    /// Searches for specific-typed music data.
    /// API Route: /api/search/cloud-search.
    /// </summary>
    [MusicApi("cloud-search")]
    internal MusicApi<SearchApiParameter, SearchApiResponse> CloudSearchApi => field ??= new(
        $"{RouteBase}/cloud-search",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = EapiHandler.Current,
            Url = "/api/cloudsearch/pc"
        },
        $"{Constants.InterfaceRequestBase}/eapi/cloudsearch/pc",
        p => new()
        {
            ["s"] = p.Keyword,
            ["type"] = (int) p.SearchTarget,
            ["limit"] = p.Paging.Limit ?? 30,
            ["offset"] = p.Paging.Offset ?? 0
        });

    /// <summary>
    /// Searches for general or specific-typed music data.
    /// </summary>
    /// <param name="keyword">Keyword for the search.</param>
    /// <param name="searchTarget">Target of the search.</param>
    /// <param name="offset">Offset of the result collection.</param>
    /// <param name="limit">Limit to the count of result entries returned for current page. Default is 30.</param>
    /// <returns>Result of the search, in a certain type that derives from <see cref="SearchApiResponse"/>.</returns>
    /// <exception cref="NotSupportedException">Search target is not supported.</exception>
    public async Task<SearchApiResponse> SearchAsync(string keyword, SearchTarget searchTarget,
        int? offset = null, int? limit = null) => searchTarget switch
    {
        SearchTarget.General => await SearchApi.RequestAsync<GeneralSearchApiResponse>
            ((keyword, searchTarget, (offset, limit))),
        SearchTarget.Album => await CloudSearchApi.RequestAsync<AlbumSearchApiResponse>
            ((keyword, searchTarget, (offset, limit))),
        SearchTarget.Artist => await CloudSearchApi.RequestAsync<ArtistSearchApiResponse>
            ((keyword, searchTarget, (offset, limit))),
        SearchTarget.Lyrics => await CloudSearchApi.RequestAsync<LyricsSearchApiResponse>
            ((keyword, searchTarget, (offset, limit))),
        SearchTarget.MusicVideo => await CloudSearchApi.RequestAsync<MusicVideoSearchApiResponse>
            ((keyword, searchTarget, (offset, limit))),
        SearchTarget.Playlist => await CloudSearchApi.RequestAsync<PlaylistSearchApiResponse>
            ((keyword, searchTarget, (offset, limit))),
        SearchTarget.PodcastChannel => await CloudSearchApi.RequestAsync<PodcastChannelSearchApiResponse>
            ((keyword, searchTarget, (offset, limit))),
        SearchTarget.Track => await CloudSearchApi.RequestAsync<TrackSearchApiResponse>
            ((keyword, searchTarget, (offset, limit))),
        SearchTarget.User => await CloudSearchApi.RequestAsync<UserSearchApiResponse>
            ((keyword, searchTarget, (offset, limit))),
        _ => throw new NotSupportedException("Search target is not supported.")
    };
}