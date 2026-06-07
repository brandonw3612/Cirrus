using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Public;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

partial class Public
{
    /// <summary>
    /// Gets track and artist charts.
    /// API Route: /api/public/charts.
    /// </summary>
    [MusicApi("charts")]
    internal MusicApi<ChartsApiResponse> ChartsApi => field ??= new(
        $"{RouteBase}/charts",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = LinuxApiHandler.Current
        },
        $"{Constants.RequestBase}/api/toplist"
    );
    
    /// <summary>
    /// Gets track and artist charts. 
    /// </summary>
    /// <returns>Track charts and artist chart.</returns>
    public Task<ChartsApiResponse> GetChartsAsync() => ChartsApi.RequestAsync();
}