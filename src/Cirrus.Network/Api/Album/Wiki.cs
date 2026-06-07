using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

partial class Album
{
    /// <summary>
    /// Gets Wiki of an album.
    /// API Route: /api/album/wiki.
    /// </summary>
    [MusicApi("wiki")]
    internal MusicApi<ulong, MusicApiResponse> WikiApi => field ??= new(
        $"{RouteBase}/wiki",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = EapiHandler.Current,
            Url = "/api/rep/ugc/album/get"
        },
        $"{Constants.RequestBase}/weapi/rep/ugc/album/get",
        p => new()
        {
            ["albumId"] = p
        }
    );
    
    /// <summary>
    /// Gets Wiki of an album.
    /// </summary>
    /// <param name="albumId">ID of the album.</param>
    /// <returns>Wiki of the album.</returns>
    public Task<MusicApiResponse> GetWikiAsync(ulong albumId) => WikiApi.RequestAsync(albumId);
}