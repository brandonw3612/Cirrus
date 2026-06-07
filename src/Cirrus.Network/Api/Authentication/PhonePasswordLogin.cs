using System.Net;
using System.Security.Cryptography;
using System.Text;
using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Authentication;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;
using Cirrus.Network.Utilities;
using PhonePasswordLoginApiParameter = (string? CountryCode, string PhoneNumber, string Password);

namespace Cirrus.Network.Api;

partial class Authentication
{
    /// <summary>
    /// Logs the user in with provided phone number and password.
    /// API Route: /api/authentication/login/phone/password.
    /// </summary>
    [MusicApi("login/phone/password")]
    internal MusicApi<PhonePasswordLoginApiParameter, LoginApiResponse> PhonePasswordLoginApi => field ??= new(
        $"{RouteBase}/login/phone/password",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current,
            UserAgentPlatform = UserAgents.Platform.Desktop,
            CookiesFix =
            {
                new Cookie("os", "ios"),
                new Cookie("appver", "8.7.01")
            }
        },
        $"{Constants.RequestBase}/weapi/login/cellphone",
        p => new()
        {
            ["phone"] = p.PhoneNumber,
            ["countrycode"] = p.CountryCode ?? "86",
            ["password"] = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(p.Password))).ToLower(),
            ["rememberLogin"] = "true"
        }
    );

    /// <summary>
    /// Logs the user in with provided phone number and password.
    /// </summary>
    /// <param name="phoneNumber">Phone number associated with the user's account.</param>
    /// <param name="password">Password of the account.</param>
    /// <param name="countryCode">Country code of the phone number, default is +86.</param>
    /// <returns>Login results.</returns>
    public Task<LoginApiResponse> PhonePasswordLoginAsync(string phoneNumber, string password,
        string? countryCode = null) =>
        PhonePasswordLoginApi.RequestAsync((countryCode, phoneNumber, password));
}