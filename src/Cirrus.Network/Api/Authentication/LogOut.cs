using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;
using Cirrus.Network.Utilities;

namespace Cirrus.Network.Api;

partial class Authentication
{
    /// <summary>
    /// Logs current user out.
    /// API Route: /api/authentication/log-out.
    /// </summary>
    [MusicApi("log-out")]
    internal MusicApi<MusicApiResponse> LogOutApi => field ??= new(
        $"{RouteBase}/log-out",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current,
            UserAgentPlatform = UserAgents.Platform.Desktop
        },
        $"{Constants.RequestBase}/weapi/logout"
    );

    /// <summary>
    /// Logs current user out.
    /// </summary>
    public Task<MusicApiResponse> LogOutAsync() => LogOutApi.RequestAsync();
}