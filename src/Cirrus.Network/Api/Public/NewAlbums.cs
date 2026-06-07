using Cirrus.Models.Network.Response.Public;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

partial class Public
{
    /// <summary>
    /// Gets latest albums released.
    /// API Route: /api/public/new-albums.
    /// </summary>
    internal MusicApi<NewAlbumsApiResponse> NewAlbumsApi => field ??= new(
        $"{RouteBase}/new-albums",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.RequestBase}/api/discovery/newAlbum"
    );
    
    /// <summary>
    /// Gets latest albums released.
    /// </summary>
    /// <returns>Latest albums.</returns>
    public Task<NewAlbumsApiResponse> GetNewAlbumsAsync() => NewAlbumsApi.RequestAsync();
}