using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Account;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;
using PersonalizedNewMusicApiParameter = (int? Limit, int? AreaId);

namespace Cirrus.Network.Api;

partial class Account
{
    /// <summary>
    /// Gets personalized new music for current user.
    /// API Route: /api/account/personalized-new-music.
    /// </summary>
    [MusicApi("personalized-new-music")]
    internal MusicApi<PersonalizedNewMusicApiParameter, PersonalizedNewMusicApiResponse> PersonalizedNewMusicApi => field ??=
        new(
            $"{RouteBase}/personalized-new-music",
            HttpMethod.Post,
            new()
            {
                EncryptionHandler = WeapiHandler.Current
            },
            $"{Constants.RequestBase}/api/personalized/newsong",
            p => new()
            {
                ["type"] = "recommend",
                ["limit"] = p.Limit ?? 30,
                ["areaId"] = p.AreaId ?? 0
            }
        );

    /// <summary>
    /// Gets personalized new music for current user.
    /// </summary>
    /// <param name="limit">Limit to the list of new tracks.</param>
    /// <param name="areaId">The ID of the area.</param>
    /// <returns>Personalized new music for current user.</returns>
    public Task<PersonalizedNewMusicApiResponse> GetPersonalizedNewMusicAsync(int? limit = null, int? areaId = null) =>
        PersonalizedNewMusicApi.RequestAsync((limit, areaId));
}