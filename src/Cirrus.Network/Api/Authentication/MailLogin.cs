using System.Net;
using System.Security.Cryptography;
using System.Text;
using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Authentication;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;
using Cirrus.Network.Utilities;
using MailLoginApiParameter = (string MailAddress, string Password);

namespace Cirrus.Network.Api;

partial class Authentication
{
    /// <summary>
    /// Logs the user in with provided NetEase Mail address and password.
    /// API Route: /api/authentication/login/mail.
    /// </summary>
    [MusicApi("login/mail")]
    internal MusicApi<MailLoginApiParameter, LoginApiResponse> MailLoginApi => field ??= new(
        $"{RouteBase}/login/mail",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current,
            CookiesFix =
            {
                new Cookie("os", "ios"),
                new Cookie("appver", "8.7.01")
            },
            UserAgentPlatform = UserAgents.Platform.Desktop
        },
        $"{Constants.RequestBase}/api/login",
        p => new()
        {
            ["username"] = p.MailAddress,
            ["password"] = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(p.Password))).ToLower(),
            ["rememberLogin"] = "true"
        }
    );

    /// <summary>
    /// Logs the user in with provided NetEase Mail address and password.
    /// </summary>
    /// <param name="mailAddress">NetEase Email address associated with the user's account.</param>
    /// <param name="password">Password of the account.</param>
    /// <returns>Login results.</returns>
    public Task<LoginApiResponse> MailLoginAsync(string mailAddress, string password) =>
        MailLoginApi.RequestAsync((mailAddress, password));
}