using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Search;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

partial class Search
{
    /// <summary>
    /// Suggest search keywords based on given keyword.
    /// API Route: /api/search/suggest.
    /// </summary>
    [MusicApi("suggest")]
    internal MusicApi<string, SuggestApiResponse> SuggestApi => field ??= new(
        $"{RouteBase}/suggest",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.RequestBase}/weapi/search/suggest/keyword",
        p => new()
        {
            ["s"] = p
        }
    );

    /// <summary>
    /// Suggest search keywords based on given keyword.
    /// </summary>
    /// <param name="keyword">Keyword given by the user.</param>
    /// <returns>Suggestions based on given keyword.</returns>
    public Task<SuggestApiResponse> SuggestKeywordsAsync(string keyword) => SuggestApi.RequestAsync(keyword);
}