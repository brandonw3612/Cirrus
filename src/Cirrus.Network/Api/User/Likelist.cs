using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.User;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

partial class User
{
    /// <summary>
    /// Gets the like list of a user.
    /// API Route: /api/user/like-list.
    /// </summary>
    [MusicApi("like-list")]
    internal MusicApi<ulong, UserLikeListApiResponse> LikeListApi => field ??= new(
        $"{RouteBase}/like-list",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.RequestBase}/weapi/song/like/get",
        p => new()
        {
            ["uid"] = p
        }
    );

    /// <summary>
    /// Gets the like list of a user.
    /// </summary>
    /// <param name="userId">ID of the user.</param>
    /// <returns>Like list of the user.</returns>
    public Task<UserLikeListApiResponse> GetLikeListAsync(ulong userId) => LikeListApi.RequestAsync(userId);
}