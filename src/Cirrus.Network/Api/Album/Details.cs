using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Album;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

partial class Album
{
    /// <summary>
    /// Gets details of an album.
    /// API Route: /api/album/details.
    /// </summary>
    [MusicApi("details")]
    internal MusicApi<ulong, AlbumDetailsApiResponse> DetailsApi => field ??= new(
        $"{RouteBase}/details",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        p => $"{Constants.RequestBase}/weapi/v1/album/{p}"
    );
    
    /// <summary>
    /// Gets details of an album.
    /// </summary>
    /// <param name="albumId">ID of the album.</param>
    /// <returns>Details of the album.</returns>
    public Task<AlbumDetailsApiResponse> GetDetailsAsync(ulong albumId) => DetailsApi.RequestAsync(albumId);
}