using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Public;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;
using TopPlaylistsApiParameter = (string? Category, int? Limit);

namespace Cirrus.Network.Api;

partial class Public
{
    /// <summary>
    /// Gets top playlists.
    /// API Route: /api/public/top-playlists.
    /// </summary>
    [MusicApi("top-playlists")]
    internal MusicApi<TopPlaylistsApiParameter, TopPlaylistsApiResponse> TopPlaylistsApi => field ??= new(
        $"{RouteBase}/top-playlists",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.RequestBase}/api/playlist/highquality/list",
        p => new()
        {
            ["cat"] = p.Category ?? "全部",
            ["limit"] = p.Limit ?? 20,
            ["lasttime"] = 0,
            ["total"] = true
        }
    );

    /// <summary>
    /// Gets top playlists.
    /// </summary>
    /// <param name="category">Category of the playlist. Default is all.</param>
    /// <param name="limit">Limit to the count of playlists returned for current page. Default is 20.</param>
    /// <returns>Top playlists.</returns>
    public Task<TopPlaylistsApiResponse> GetTopPlaylistsAsync(string? category = null, int? limit = null) =>
        TopPlaylistsApi.RequestAsync((category, limit));
}