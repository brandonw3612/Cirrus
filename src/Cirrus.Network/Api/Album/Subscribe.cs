using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;
using SubscribeApiParameter = (ulong AlbumId, bool IsToSubscribe);

namespace Cirrus.Network.Api;

partial class Album
{
    /// <summary>
    /// Subscribes or unsubscribes an album.
    /// API Route: /api/album/subscribe.
    /// </summary>
    [MusicApi("subscribe")]
    internal MusicApi<SubscribeApiParameter, MusicApiResponse> SubscribeApi => field ??= new(
        $"{RouteBase}/subscribe",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        p => $"{Constants.RequestBase}/api/album/" + (p.IsToSubscribe ? "sub" : "unsub"),
        p => new()
        {
            ["id"] = p.AlbumId
        }
    );

    /// <summary>
    /// Subscribes an album.
    /// </summary>
    /// <param name="albumId">ID of the album.</param>
    /// <returns>Whether the operation succeeded.</returns>
    public async Task<bool> SubscribeAsync(ulong albumId) =>
        await SubscribeApi.RequestAsync((albumId, true)) is {StatusCode: 200};

    /// <summary>
    /// Unsubscribes an album.
    /// </summary>
    /// <param name="albumId">ID of the album.</param>
    /// <returns>Whether the operation succeeded.</returns>
    public async Task<bool> UnsubscribeAsync(ulong albumId) =>
        await SubscribeApi.RequestAsync((albumId, false)) is {StatusCode: 200};
}