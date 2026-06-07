using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Public;
using Cirrus.Models.Shared.Public;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

partial class Public
{
    /// <summary>
    /// Gets latest tracks released.
    /// API Route: /api/public/new-tracks.
    /// </summary>
    [MusicApi("new-tracks")]
    internal MusicApi<PublishArea?, NewTracksApiResponse> NewTracksApi => field ??= new(
        $"{RouteBase}/new-tracks",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.RequestBase}/weapi/v1/discovery/new/songs",
        p => new()
        {
            ["areaId"] = (int) (p ?? PublishArea.Unspecified),
            ["total"] = true
        }
    );
    
    /// <summary>
    /// Gets latest tracks released.
    /// </summary>
    /// <param name="area">Area of the query. Default is All.</param>
    /// <returns>Latest tracks of the specified area.</returns>
    public Task<NewTracksApiResponse> GetNewTracksAsync(PublishArea? area = null) => NewTracksApi.RequestAsync(area);
}