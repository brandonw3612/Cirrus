using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Account;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;
using PagingParameter = (int? Offset, int? Limit);

namespace Cirrus.Network.Api;

partial class Account
{
    /// <summary>
    /// Gets current user's subscribed albums.
    /// API Route: /api/account/subscribed-albums.
    /// </summary>
    [MusicApi("subscribed-albums")]
    internal MusicApi<PagingParameter, SubscribedAlbumsApiResponse> SubscribedAlbumsApi => field ??= new(
        $"{RouteBase}/subscribed-albums",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.RequestBase}/weapi/album/sublist",
        p => new()
        {
            ["limit"] = p.Limit ?? 25,
            ["offset"] = p.Offset ?? 0,
            ["total"] = true
        }
    );

    /// <summary>
    /// Gets current user's subscribed albums.
    /// </summary>
    /// <param name="offset">Offset of the album collection. Default is 0.</param>
    /// <param name="limit">Limit to the count of albums returned for current page. Default is 25.</param>
    /// <returns>User's subscribed albums.</returns>
    public Task<SubscribedAlbumsApiResponse> GetSubscribedAlbumsAsync(int? offset = null, int? limit = null) =>
        SubscribedAlbumsApi.RequestAsync((offset, limit));
}