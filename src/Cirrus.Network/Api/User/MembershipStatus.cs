using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.User;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

partial class User
{
    /// <summary>
    /// Gets membership status for the specified user.
    /// API Route: /api/user/membership-status.
    /// </summary>
    [MusicApi("membership-status")]
    internal MusicApi<ulong, UserMembershipStatusApiResponse> MembershipStatusApi => field ??= new(
        $"{RouteBase}/membership-status",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.RequestBase}/weapi/music-vip-membership/client/vip/info",
        userId => new()
        {
            ["userId"] = userId.ToString()
        }
    );

    /// <summary>
    /// Gets membership status for the specified user.
    /// </summary>
    /// <param name="userId">ID of the user.</param>
    /// <returns>User membership status.</returns>
    public Task<UserMembershipStatusApiResponse> GetMembershipStatusAsync(ulong userId) => MembershipStatusApi.RequestAsync(userId);
}