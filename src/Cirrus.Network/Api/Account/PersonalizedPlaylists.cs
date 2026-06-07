using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Account;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

partial class Account
{
    /// <summary>
    /// Gets personalized playlists for current user.
    /// API Route: /api/account/personalized-playlists.
    /// </summary>
    [MusicApi("personalized-playlists")]
    internal MusicApi<int?, PersonalizedPlaylistsApiResponse> PersonalizedPlaylistsApi => field ??=
        new(
            $"{RouteBase}/personalized-playlists",
            HttpMethod.Post,
            new()
            {
                EncryptionHandler = WeapiHandler.Current
            },
            $"{Constants.RequestBase}/weapi/personalized/playlist",
            p => new()
            {
                ["limit"] = p ?? 30,
                ["total"] = true,
                ["n"] = 1000
            }
        );

    /// <summary>
    /// Gets personalized playlists for current user.
    /// </summary>
    /// <param name="limit">Limit to the list of playlists.</param>
    /// <returns>Personalized playlists for current user.</returns>
    public Task<PersonalizedPlaylistsApiResponse> GetPersonalizedPlaylistsAsync(int? limit = null) =>
        PersonalizedPlaylistsApi.RequestAsync(limit);
}