using System.Net;
using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Account;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

partial class Account
{
    /// <summary>
    /// Gets daily mix playlist for current user.
    /// API Route: /api/account/daily-mix.
    /// </summary>
    [MusicApi("daily-mix")]
    internal MusicApi<DailyMixApiResponse> DailyMixApi => field ??= new(
        $"{RouteBase}/daily-mix",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current,
            CookiesFix =
            {
                new Cookie("os", "ios")
            }
        },
        $"{Constants.RequestBase}/api/v3/discovery/recommend/songs"
    );

    /// <summary>
    /// Gets daily mix playlist for current user.
    /// </summary>
    /// <returns>Daily mix playlist.</returns>
    public Task<DailyMixApiResponse> GetDailyMixAsync() => DailyMixApi.RequestAsync();
}