using System.Text.Json;
using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;
using Cirrus.Network.Serialization;
using ManipulateTracksApiParameter = (ulong PlaylistId, System.Collections.Generic.List<ulong> TrackIds, bool IsToAdd);

namespace Cirrus.Network.Api;

partial class Playlist
{
    /// <summary>
    /// Adds/removes tracks to/from the specified playlist.
    /// API Route: /api/playlist/manipulate-tracks.
    /// </summary>
    [MusicApi("manipulate-tracks")]
    internal MusicApi<ManipulateTracksApiParameter, MusicApiResponse> ManipulateTracksApi => field ??= new(
        $"{RouteBase}/track/manipulate",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.RequestBase}/weapi/playlist/manipulate/tracks",
        p => new()
        {
            ["op"] = p.IsToAdd ? "add" : "del",
            ["pid"] = p.PlaylistId,
            ["trackIds"] = JsonSerializer.Serialize<List<ulong>>(p.TrackIds, NetworkSerializationContext.Default.ListUInt64),
            ["imme"] = "true"
        }
    );

    /// <summary>
    /// Adds tracks to the specified playlist.
    /// </summary>
    /// <param name="playlistId">ID of the playlist.</param>
    /// <param name="trackIds">IDs of the tracks.</param>
    /// <returns>Whether the operation succeeded.</returns>
    public async Task<bool> AddTracksAsync(ulong playlistId, List<ulong> trackIds) =>
        await ManipulateTracksApi.RequestAsync((playlistId, trackIds, true)) is {StatusCode: 200};

    /// <summary>
    /// Removes tracks from the specified playlist.
    /// </summary>
    /// <param name="playlistId">ID of the playlist.</param>
    /// <param name="trackIds">IDs of the tracks.</param>
    /// <returns>Whether the operation succeeded.</returns>
    public async Task<bool> RemoveTracksAsync(ulong playlistId, List<ulong> trackIds) =>
        await ManipulateTracksApi.RequestAsync((playlistId, trackIds, false)) is {StatusCode: 200};
}