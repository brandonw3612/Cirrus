using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Authentication;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

partial class Authentication
{
    /// <summary>
    /// Gets QR Code key to log user in.
    /// API Route: /api/authentication/login/qr/key.
    /// </summary>
    [MusicApi("login/qr/key")]
    internal MusicApi<LoginQrCodeKeyApiResponse> LoginQrCodeKeyApi => field ??= new(
        $"{RouteBase}/login/qr/key",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.RequestBase}/weapi/login/qrcode/unikey",
        new()
        {
            ["type"] = 1
        }
    );

    /// <summary>
    /// Gets QR Code key to log user in.
    /// </summary>
    /// <returns>QR Code key to log user in.</returns>
    public Task<LoginQrCodeKeyApiResponse> GetLoginQrCodeKeyAsync() => LoginQrCodeKeyApi.RequestAsync();
}