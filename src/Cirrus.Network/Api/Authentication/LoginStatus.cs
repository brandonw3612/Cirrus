using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Authentication;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

partial class Authentication
{
    /// <summary>
    /// Gets log in status of the client.
    /// API Route: /api/authentication/login/status.
    /// </summary>
    [MusicApi("login/status")]
    internal MusicApi<LoginStatusApiResponse> LoginStatusApi => field ??= new(
        $"{RouteBase}/login/status",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.RequestBase}/weapi/w/nuser/account/get"
    );

    /// <summary>
    /// Gets log in status of the client.
    /// </summary>
    /// <returns>Log in status of the client.</returns>
    public Task<LoginStatusApiResponse> GetLoginStatusAsync() => LoginStatusApi.RequestAsync();
}