using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Artist;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

partial class Artist
{
    /// <summary>
    /// Gets artist details and hot tracks.
    /// API Route: /api/artist/details-tracks
    /// </summary>
    [MusicApi("details-tracks")]
    internal MusicApi<ulong, ArtistDetailsTracksApiResponse> DetailsTracksApi => field ??= new(
        $"{RouteBase}/details-tracks",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        p => $"{Constants.RequestBase}/weapi/v1/artist/{p}"
    );

    /// <summary>
    /// Gets artist details and hot tracks.
    /// </summary>
    /// <param name="artistId">ID of the artist.</param>
    /// <returns>Details and hot tracks of the artist.</returns>
    public Task<ArtistDetailsTracksApiResponse> GetDetailsAndTracksAsync(ulong artistId) =>
        DetailsTracksApi.RequestAsync(artistId);
}