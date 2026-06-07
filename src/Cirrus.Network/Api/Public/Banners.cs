using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Public;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

partial class Public
{
    /// <summary>
    /// Gets banners on the Home page.
    /// API Route: /api/public/banners.
    /// </summary>
    [MusicApi("banners")]
    internal MusicApi<BannersApiResponse> BannersApi => field ??= new(
        $"{RouteBase}/banners",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = LinuxApiHandler.Current
        },
        $"{Constants.RequestBase}/api/v2/banner/get",
        new()
        {
            ["clientType"] = "pc"
        }
    );
    
    /// <summary>
    /// Gets banners on the Home page.
    /// </summary>
    /// <returns>Banners on the home page.</returns>
    public Task<BannersApiResponse> GetBannersAsync() => BannersApi.RequestAsync();
}