using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Artist;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

partial class Artist
{
    /// <summary>
    /// Gets similar artists.
    /// API Route: /api/artist/similar.
    /// </summary>
    [MusicApi("similar")]
    internal MusicApi<ulong, SimilarArtistsApiResponse> SimilarApi => field ??= new(
        $"{RouteBase}/similar",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        }, 
        $"{Constants.RequestBase}/weapi/discovery/simiArtist",
        static p => new()
        {
            ["artistid"] = p
        }
    );

    /// <summary>
    /// Gets similar artists.
    /// </summary>
    /// <param name="artistId">ID of the artist.</param>
    /// <returns>Similar artists.</returns>
    public Task<SimilarArtistsApiResponse> GetSimilarArtistsAsync(ulong artistId) =>
        SimilarApi.RequestAsync(artistId);
}