using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Playlist;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;
using DetailsApiParameter = (ulong PlaylistId, int? SubscriberLimit);

namespace Cirrus.Network.Api;

partial class Playlist
{
    /// <summary>
    /// Gets the details of a playlist.
    /// API Route: /api/playlist/details.
    /// </summary>
    [MusicApi("details")]
    internal MusicApi<DetailsApiParameter, PlaylistDetailsApiResponse> DetailsApi => field ??= new(
        $"{RouteBase}/details",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = LinuxApiHandler.Current
        },
        $"{Constants.RequestBase}/api/v6/playlist/detail",
        p => new()
        {
            ["id"] = p.PlaylistId,
            ["n"] = 100_000,
            ["s"] = p.SubscriberLimit ?? 8
        }
    );

    /// <summary>
    /// Gets the details of a playlist.
    /// </summary>
    /// <param name="playlistId">Id of the playlist.</param>
    /// <param name="subscriberLimit">Limit to the subscriber list of the playlist.</param>
    /// <returns>Details of the playlist.</returns>
    public Task<PlaylistDetailsApiResponse> GetDetailsAsync(ulong playlistId, int? subscriberLimit = null) =>
        DetailsApi.RequestAsync((playlistId, subscriberLimit));
}