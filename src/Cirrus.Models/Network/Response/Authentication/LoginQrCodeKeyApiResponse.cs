using System.Text.Json.Serialization;

namespace Cirrus.Models.Network.Response.Authentication;

/// <summary>
/// Response for API authentication/login/qr/key.
/// </summary>
public class LoginQrCodeKeyApiResponse : MusicApiResponse
{
    [JsonPropertyName("unikey")] public string? Key { get; init; }
}