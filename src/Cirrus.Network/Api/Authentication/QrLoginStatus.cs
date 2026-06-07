using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

partial class Authentication
{
    /// <summary>
    /// Gets status of the login QR Code.
    /// API Route: /api/authentication/login/qr/status.
    /// </summary>
    [MusicApi("login/qr/status")]
    internal MusicApi<string, MusicApiResponse> QrLoginStatusApi => field ??= new(
        $"{RouteBase}/login/qr/status",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.RequestBase}/weapi/login/qrcode/client/login",
        key => new()
        {
            ["key"] = key,
            ["type"] = 1
        }
    );

    /// <summary>
    /// Gets status of the login QR Code.
    /// </summary>
    /// <returns>
    /// The status code of the response indicates the status of the login QR Code. In which
    /// 800 - QR code expired;
    /// 801 - QR code ready to be scanned;
    /// 802 - QR code scanned, waiting for confirmation;
    /// 803 - Authenticated.
    /// </returns>
    public Task<MusicApiResponse> GetQrLoginStatusAsync(string codeKey) => QrLoginStatusApi.RequestAsync(codeKey);
}