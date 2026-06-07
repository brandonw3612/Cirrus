using System.Net;
using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;
using FollowApiParameter = (ulong UserId, bool IsToFollow);
    
namespace Cirrus.Network.Api;

partial class User
{
    /// <summary>
    /// Follows or unfollows a user.
    /// API Route: /api/user/follow.
    /// </summary>
    [MusicApi("follow")]
    internal MusicApi<FollowApiParameter, MusicApiResponse> FollowApi => field ??= new(
        $"{RouteBase}/follow",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current,
            CookiesFix =
            {
                new Cookie("os", "pc")
            }
        },
        p => $"{Constants.RequestBase}/weapi/user/"
             + (p.IsToFollow ? "follow" : "delfollow") + $"/{p.UserId}"
    );

    /// <summary>
    /// Follows a user.
    /// </summary>
    /// <param name="userId">ID of the user to follow.</param>
    /// <returns>Whether the operation succeeded.</returns>
    public async Task<bool> FollowAsync(ulong userId) =>
        await FollowApi.RequestAsync((userId, true)) is {StatusCode: 200};

    /// <summary>
    /// Unfollows a user.
    /// </summary>
    /// <param name="userId">ID of the user to unfollow.</param>
    /// <returns>Whether the operation succeeded.</returns>
    public async Task<bool> UnfollowAsync(ulong userId) =>
        await FollowApi.RequestAsync((userId, false)) is {StatusCode: 200};
}