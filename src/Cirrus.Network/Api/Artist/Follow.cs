using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;
using FollowApiParameter = (ulong ArtistId, bool IsToFollow);

namespace Cirrus.Network.Api;

partial class Artist
{
    /// <summary>
    /// Follows or unfollows an artist.
    /// API Route: /api/artist/follow.
    /// </summary>
    [MusicApi("follow")]
    internal MusicApi<FollowApiParameter, MusicApiResponse> FollowApi => field ??= new(
        $"{RouteBase}/follow",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        p => $"{Constants.RequestBase}/weapi/artist/" + (p.IsToFollow ? "sub" : "unsub"),
        p => new()
        {
            ["artistId"] = p.ArtistId,
            ["artistIds"] = $"[{p.ArtistId}]"
        }
    );

    /// <summary>
    /// Follows an artist.
    /// </summary>
    /// <param name="artistId">ID of the artist to follow.</param>
    /// <returns>Whether the operation succeeded.</returns>
    public async Task<bool> FollowAsync(ulong artistId) =>
        await FollowApi.RequestAsync((artistId, true)) is {StatusCode: 200};

    /// <summary>
    /// Unfollows an artist.
    /// </summary>
    /// <param name="artistId">ID of the artist to unfollow.</param>
    /// <returns>Whether the operation succeeded.</returns>
    public async Task<bool> UnfollowAsync(ulong artistId) =>
        await FollowApi.RequestAsync((artistId, false)) is {StatusCode: 200};
}