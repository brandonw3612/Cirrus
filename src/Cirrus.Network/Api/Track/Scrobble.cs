using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;
using ScrobbleApiParameter = (ulong TrackId, ulong SourceId, System.TimeSpan ScrobbleTime);

namespace Cirrus.Network.Api;

partial class Track
{
    /// <summary>
    /// Scrobbles a track listening record.
    /// API Route: /api/track/scrobble.
    /// </summary>
    [MusicApi("scrobble")]
    internal MusicApi<ScrobbleApiParameter, MusicApiResponse> ScrobbleApi => field ??= new(
        $"{RouteBase}/scrobble",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.RequestBase}/weapi/feedback/weblog",
        p => new()
        {
            ["logs"] = new Dictionary<string, object>[]
            {
                new()
                {
                    ["action"] = "play",
                    ["json"] = new Dictionary<string, object>
                    {
                        ["download"] = 0,
                        ["end"] = "playend",
                        ["id"] = p.TrackId,
                        ["sourceId"] = p.SourceId,
                        ["time"] = (int) p.ScrobbleTime.TotalSeconds,
                        ["type"] = "song",
                        ["wifi"] = 0,
                        ["source"] = "list"
                    }
                }
            }
        }
    );
    
    /// <summary>
    /// Scrobbles a track listening record.
    /// </summary>
    /// <param name="trackId">ID of the track.</param>
    /// <param name="sourceId">Source of the track. Usually the playlist or the album.</param>
    /// <param name="scrobbleTime">Time the track has been played.</param>
    public Task ScrobbleAsync(ulong trackId, ulong sourceId, TimeSpan scrobbleTime) =>
        ScrobbleApi.RequestAsync((trackId, sourceId, scrobbleTime));
}