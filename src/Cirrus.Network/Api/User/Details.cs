using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.User;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

partial class User
{
    /// <summary>
    /// Gets user account info and profile.
    /// API Route: /api/user/details.
    /// </summary>
    [MusicApi("details")]
    internal MusicApi<ulong, UserDetailsApiResponse> DetailsApi => field ??= new(
        $"{RouteBase}/details",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        userId => $"{Constants.RequestBase}/weapi/v1/user/detail/{userId}"
    );
    
    /// <summary>
    /// Gets user account info and profile.
    /// </summary>
    /// <param name="userId">ID of the user.</param>
    /// <returns>User details with account info and profile.</returns>
    public Task<UserDetailsApiResponse> GetDetailsAsync(ulong userId) => DetailsApi.RequestAsync(userId);
}