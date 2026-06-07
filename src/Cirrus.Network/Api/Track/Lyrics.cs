using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Track;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

partial class Track
{
    /// <summary>
    /// Gets the lyrics of a track.
    /// API Route: /api/track/lyrics.
    /// </summary>
    [MusicApi("lyrics")]
    internal MusicApi<ulong, LyricsApiResponse> LyricsApi => field ??= new(
        $"{RouteBase}/lyrics",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = EapiHandler.Current,
            Url = "/api/song/lyric/v1"
        },
        $"{Constants.Interface3RequestBase}/eapi/song/lyric/v1",
        p => new()
        {
            ["id"] = p,
            ["cp"] = false,
            ["tv"] = 0,
            ["lv"] = 0,
            ["rv"] = 0,
            ["kv"] = 0,
            ["yv"] = 0,
            ["ytv"] = 0,
            ["yrv"] = 0
        }
    );

    /// <summary>
    /// Gets the lyrics of a track.
    /// </summary>
    /// <param name="trackId">ID of the track.</param>
    /// <returns>Lyrics of the specified track.</returns>
    public Task<LyricsApiResponse> GetLyricsAsync(ulong trackId) => LyricsApi.RequestAsync(trackId);
}