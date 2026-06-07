using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;
using SubscribeApiParameter = (ulong PlaylistId, bool IsToSubscribe);

namespace Cirrus.Network.Api;

partial class Playlist
{
    /// <summary>
    /// Subscribes or unsubscribes a playlist for current user.
    /// API Route: /api/playlist/subscribe.
    /// </summary>
    [MusicApi("subscribe")]
    internal MusicApi<SubscribeApiParameter, MusicApiResponse> SubscribeApi => field ??= new(
        $"{RouteBase}/subscribe",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        p => $"{Constants.RequestBase}/weapi/playlist/{(p.IsToSubscribe ? "subscribe" : "unsubscribe")}",
        p => new()
        {
            ["id"] = p.PlaylistId
        }
    );

    /// <summary>
    /// Subscribes a playlist for current user.
    /// </summary>
    /// <param name="playlistId">Id of the playlist</param>
    /// <returns>Whether the operation succeeded.</returns>
    public async Task<bool> SubscribeAsync(ulong playlistId) =>
        await SubscribeApi.RequestAsync((playlistId, true)) is {StatusCode: 200};
   
    /// <summary>
    /// Unsubscribes a playlist for current user.
    /// </summary>
    /// <param name="playlistId">Id of the playlist</param>
    /// <returns>Whether the operation succeeded.</returns>
    public async Task<bool> UnsubscribeAsync(ulong playlistId) =>
        await SubscribeApi.RequestAsync((playlistId, false)) is {StatusCode: 200};
}