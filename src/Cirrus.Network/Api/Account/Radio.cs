using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Account;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

partial class Account
{
    /// <summary>
    /// Gets tracks from Personal Radio Station.
    /// API Route: /api/account/radio.
    /// </summary>
    [MusicApi("radio")]
    internal MusicApi<RadioApiResponse> RadioApi => field ??= new(
        $"{RouteBase}/radio",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.RequestBase}/weapi/v1/radio/get"
    );
    
    /// <summary>
    /// Gets tracks from Personal Radio Station.
    /// </summary>
    /// <returns>3 tracks from Personal Radio Station.</returns>
    public Task<RadioApiResponse> GetRadioTracks() => RadioApi.RequestAsync();
}