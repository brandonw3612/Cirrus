using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Account;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;
using PagingParameter = (int? Offset, int? Limit);

namespace Cirrus.Network.Api;

partial class Account
{
    /// <summary>
    /// Gets current user's subscribed artists.
    /// API Route: /api/account/subscribed-artists.
    /// </summary>
    [MusicApi("subscribed-artists")]
    internal MusicApi<PagingParameter, SubscribedArtistsApiResponse> SubscribedArtistsApi => field ??= new(
        $"{RouteBase}/subscribed-artists",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.RequestBase}/weapi/artist/sublist",
        p => new()
        {
            ["limit"] = p.Limit ?? 25,
            ["offset"] = p.Offset ?? 0,
            ["total"] = true
        }
    );

    /// <summary>
    /// Gets current user's subscribed artists.
    /// </summary>
    /// <param name="offset">Offset of the artist collection. Default is 0.</param>
    /// <param name="limit">Limit to the count of artists returned for current page. Default is 25.</param>
    /// <returns>User's subscribed artists.</returns>
    public Task<SubscribedArtistsApiResponse> GetSubscribedArtistsAsync(int? offset = null, int? limit = null) =>
        SubscribedArtistsApi.RequestAsync((offset, limit));
}