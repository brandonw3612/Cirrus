using Cirrus.Models.Network.Response.Public;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;
using PagingParameter = (int? Limit, int? Offset);

namespace Cirrus.Network.Api;

partial class Public
{
    /// <summary>
    /// Gets top artists.
    /// API Route: /api/public/top-artists.
    /// </summary>
    internal MusicApi<PagingParameter, TopArtistsApiResponse> TopArtistsApi => field ??= new(
        $"{RouteBase}/top-artists",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.RequestBase}/weapi/artist/top",
        p => new()
        {
            ["limit"] = p.Limit ?? 50,
            ["offset"] = p.Offset ?? 0,
            ["total"] = true
        }
    );

    /// <summary>
    /// Gets top artists.
    /// </summary>
    /// <param name="limit">Maximum number of artists to return. Default is 50.</param>
    /// <param name="offset">Offset of the query. Default is 0.</param>
    /// <returns>Top artists.</returns>
    public Task<TopArtistsApiResponse> GetTopArtistsAsync(int? limit = null, int? offset = null) =>
        TopArtistsApi.RequestAsync((limit, offset));
}