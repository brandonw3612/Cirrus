using System.Net;
using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;
using LikeApiParameter = (ulong TrackId, bool IsToLike);

namespace Cirrus.Network.Api;

partial class Track
{
    /// <summary>
    /// Likes or unlikes a track.
    /// API Route: /api/track/like.
    /// </summary>
    [MusicApi("like")]
    internal MusicApi<LikeApiParameter, MusicApiResponse> LikeApi => field ??= new(
        $"{RouteBase}/like",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current,
            CookiesFix =
            {
                new Cookie("os", "pc"),
                new Cookie("appver", "2.9.7")
            }
        },
        $"{Constants.RequestBase}/api/radio/like",
        p => new()
        {
            ["alg"] = "itembased",
            ["trackId"] = p.TrackId,
            ["like"] = p.IsToLike,
            ["time"] = "3"
        }
    );
    
    /// <summary>
    /// Likes a track.
    /// </summary>
    /// <param name="trackId">ID of the track.</param>
    /// <returns>Whether the operation succeeded.</returns>
    public async Task<bool> LikeAsync(ulong trackId) =>
        await LikeApi.RequestAsync((trackId, true)) is {StatusCode: 200};

    /// <summary>
    /// Unlikes a track.
    /// </summary>
    /// <param name="trackId">ID of the track.</param>
    /// <returns>Whether the operation succeeded.</returns>
    public async Task<bool> UnlikeAsync(ulong trackId) =>
        await LikeApi.RequestAsync((trackId, false)) is {StatusCode: 200};
}