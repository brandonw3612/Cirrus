using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Playlist;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;
using IntelligentListApiParameter = (ulong TrackId, ulong PlaylistId, int? Count);

namespace Cirrus.Network.Api;

partial class Playlist
{
    /// <summary>
    /// Gets intelligent list from a playlist and starting track.
    /// API Route: /api/playlist/intelligent-list.
    /// </summary>
    [MusicApi("intelligent-list")]
    internal MusicApi<IntelligentListApiParameter, IntelligentPlaylistApiResponse> IntelligentListApi => field ??= new(
        $"{RouteBase}/intelligent-list",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.InterfaceRequestBase}/weapi/playmode/intelligence/list",
        p => new()
        {
            ["songId"] = p.TrackId,
            ["type"] = "fromPlayOne",
            ["playlistId"] = p.PlaylistId,
            ["startMusicId"] = p.TrackId,
            ["count"] = p.Count ?? 1
        }
    );

    /// <summary>
    /// Gets intelligent list from a playlist and starting track.
    /// </summary>
    /// <param name="trackId">ID of the starting track in the playlist.</param>
    /// <param name="playlistId">ID of the playlist</param>
    /// <param name="count">Count of the generated intelligent playlist.</param>
    /// <returns>Intelligent playlist based on recommendation.</returns>
    public Task<IntelligentPlaylistApiResponse> GetIntelligentListAsync(ulong trackId, ulong playlistId, int? count = null)
        => IntelligentListApi.RequestAsync((trackId, playlistId, count));
}