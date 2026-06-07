using System.Net;
using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

partial class Playlist
{
    /// <summary>
    /// Removes a playlist from current user's library.
    /// API Route: /api/playlist/remove.
    /// </summary>
    [MusicApi("remove")]
    internal MusicApi<ulong, MusicApiResponse> RemoveApi => field ??= new(
        $"{RouteBase}/remove",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current,
            CookiesFix = { new Cookie("os", "pc") }
        },
        $"{Constants.RequestBase}/weapi/playlist/remove",
        p => new()
        {
            ["ids"] = $"[{p}]"
        }
    );

    /// <summary>
    /// Removes a playlist from current user's library.
    /// </summary>
    /// <param name="playlistId">ID of the playlist, which must be created by current user.</param>
    /// <returns>Whether the operation succeeded.</returns>
    public async Task<bool> RemoveAsync(ulong playlistId) => await RemoveApi.RequestAsync(playlistId) is {StatusCode: 200};
}