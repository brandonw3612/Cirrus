using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Account;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

partial class Account
{
    /// <summary>
    /// Gets account and profile for current user.
    /// API Route: /api/account/current-user.
    /// </summary>
    [MusicApi("current-user")]
    internal MusicApi<CurrentUserApiResponse> CurrentUserApi => field ??= new(
        $"{RouteBase}/current-user",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.RequestBase}/api/nuser/account/get"
    );

    /// <summary>
    /// Gets account and profile for current user.
    /// </summary>
    /// <returns>Account and profile of current user.</returns>
    public Task<CurrentUserApiResponse> GetCurrentUserAsync() => CurrentUserApi.RequestAsync();
}