using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Album;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

partial class Album
{
    /// <summary>
    /// Gets dynamic details of an album.
    /// API Route: /api/album/dynamic.
    /// </summary>
    [MusicApi("dynamic")]
    internal MusicApi<ulong, AlbumDynamicApiResponse> DynamicApi => field ??= new(
        $"{RouteBase}/dynamic",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.RequestBase}/api/album/detail/dynamic",
        p => new()
        {
            ["id"] = p
        }
    );
    
    /// <summary>
    /// Gets dynamic details of an album.
    /// </summary>
    /// <param name="albumId">ID of the album.</param>
    /// <returns>Dynamic details of the album.</returns>
    public Task<AlbumDynamicApiResponse> GetDynamicInfoAsync(ulong albumId) => DynamicApi.RequestAsync(albumId);
}