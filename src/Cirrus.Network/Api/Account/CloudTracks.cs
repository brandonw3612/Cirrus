using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Account;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;
using PagingParameter = (int? Offset, int? Limit);

namespace Cirrus.Network.Api;

partial class Account
{
    /// <summary>
    /// Gets tracks in current user's cloud drive.
    /// API Route: /api/account/cloud-tracks.
    /// </summary>
    [MusicApi("cloud-tracks")]
    internal MusicApi<PagingParameter, CloudTracksApiResponse> CloudTracksApi => field ??= new(
        $"{RouteBase}/cloud-tracks",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.RequestBase}/api/v1/cloud/get",
        p => new()
        {
            ["limit"] = p.Limit ?? 30,
            ["offset"] = p.Offset ?? 0
        }
    );
    
    /// <summary>
    /// Gets tracks in current user's cloud drive.
    /// </summary>
    /// <param name="offset">Offset of the track collection. Default is 0.</param>
    /// <param name="limit">Limit to the count of tracks returned for current page. Default is 30.</param>
    /// <returns>Tracks in current user's cloud drive.</returns>
    public Task<CloudTracksApiResponse> GetCloudTracksAsync(int? offset = null, int? limit = null) =>
        CloudTracksApi.RequestAsync((offset, limit));
}