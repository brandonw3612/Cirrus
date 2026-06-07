using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.User;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;
using PlaylistsApiParameter = (ulong UserId, (int? Offset, int? Limit) Paging);

namespace Cirrus.Network.Api;

partial class User
{
    /// <summary>
    /// Gets playlists created and saved by the user.
    /// API Route: /api/user/playlists.
    /// </summary>
    [MusicApi("playlists")]
    internal MusicApi<PlaylistsApiParameter, UserPlaylistsApiResponse> PlaylistsApi => field ??= new(
        $"{RouteBase}/playlists",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.RequestBase}/api/user/playlist",
        p => new()
        {
            ["uid"] = p.UserId,
            ["limit"] = p.Paging.Limit ?? 30,
            ["offset"] = p.Paging.Offset ?? 0
        }
    );
    
    /// <summary>
    /// Gets playlists created and saved by the user.
    /// </summary>
    /// <param name="userId">Id of the user.</param>
    /// <param name="offset">Offset of the playlist collection. Default is 0.</param>
    /// <param name="limit">Limit to the count of playlists returned for current page. Default is 30.</param>
    /// <returns>Paged data for user's playlists.</returns>
    public Task<UserPlaylistsApiResponse> GetPlaylistsAsync(ulong userId, int? offset = null, int? limit = null) =>
        PlaylistsApi.RequestAsync((userId, (offset, limit)));
}