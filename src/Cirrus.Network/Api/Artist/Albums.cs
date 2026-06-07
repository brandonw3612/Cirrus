using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Artist;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;
using AlbumsApiParameter = (ulong ArtistId, (int? Offset, int? Limit) Paging);

namespace Cirrus.Network.Api;

partial class Artist
{
    /// <summary>
    /// Gets albums performed by the specified artist.
    /// API Route: /api/artist/albums.
    /// </summary>
    [MusicApi("albums")]
    internal MusicApi<AlbumsApiParameter, ArtistAlbumApiResponse> AlbumsApi => field ??= new(
        $"{RouteBase}/albums",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        p => $"{Constants.RequestBase}/weapi/artist/albums/{p.ArtistId}",
        p => new()
        {
            ["limit"] = p.Paging.Limit ?? 30,
            ["offset"] = p.Paging.Offset ?? 0,
            ["total"] = true
        }
    );

    /// <summary>
    /// Gets albums performed by the specified artist.
    /// </summary>
    /// <param name="artistId">Id of the artist.</param>
    /// <param name="offset">Offset of the album collection. Default is 0.</param>
    /// <param name="limit">Limit to the count of albums returned for current page. Default is 30.</param>
    /// <returns>Albums performed by the specified artist.</returns>
    public Task<ArtistAlbumApiResponse> GetAlbumsAsync(ulong artistId, int? offset = null, int? limit = null) =>
        AlbumsApi.RequestAsync((artistId, (offset, limit)));
}