using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Search;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

partial class Search
{
    /// <summary>
    /// Gets trending search keywords.
    /// API Route: /api/search/trend.
    /// </summary>
    [MusicApi("trend")]
    internal MusicApi<TrendApiResponse> TrendApi => field ??= new(
        $"{RouteBase}/trend",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.RequestBase}/weapi/hotsearchlist/get"
    );

    /// <summary>
    /// Gets trending search keywords.
    /// </summary>
    /// <returns>Currently trending search keywords.</returns>
    public Task<TrendApiResponse> GetTrendingKeywordsAsync() => TrendApi.RequestAsync();
}