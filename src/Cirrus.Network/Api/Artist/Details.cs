using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Artist;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

partial class Artist
{
    /// <summary>
    /// Gets details of an artist.
    /// API Route: /api/artist/details.
    /// </summary>
    [MusicApi("details")]
    internal MusicApi<ulong, ArtistDetailsApiResponse> DetailsApi => field ??= new(
        $"{RouteBase}/details",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.RequestBase}/api/artist/head/info/get",
        p => new()
        {
            ["id"] = p
        }
    );

    /// <summary>
    /// Gets details of an artist.
    /// </summary>
    /// <param name="artistId">ID of the artist.</param>
    /// <returns>Detail of the specified artist.</returns>
    [Obsolete("This API does not return full information of the artist. Use /api/artist/details-tracks instead. However, the corresponding API can still be used in batch requests.")]
    public Task<ArtistDetailsApiResponse> GetDetailsAsync(ulong artistId) => DetailsApi.RequestAsync(artistId);
}