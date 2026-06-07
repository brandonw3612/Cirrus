using System.Net;
using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Track;
using Cirrus.Models.Shared.Track;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;
using AudioApiParameter = (ulong TrackId, Cirrus.Models.Shared.Track.AudioQuality AudioQuality); 

namespace Cirrus.Network.Api;

partial class Track
{
    /// <summary>
    /// Gets audio of a track in specific quality.
    /// API Route: /api/track/audio.
    /// </summary>
    [MusicApi("audio")]
    internal MusicApi<AudioApiParameter, AudioApiResponse> AudioApi => field ??= new(
        $"{RouteBase}/audio",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = EapiHandler.Current,
            CookiesFix =
            {
                new Cookie("os", "android"),
                new Cookie("appver", "8.10.05")
            },
            Url = "/api/song/enhance/player/url/v1"
        },
        $"{Constants.InterfaceRequestBase}/eapi/song/enhance/player/url/v1",
        p =>
        {
            Dictionary<string, object> body = new()
            {
                ["ids"] = $"[{p.TrackId}]",
                ["level"] = p.AudioQuality switch
                {
                    AudioQuality.Standard => "standard",
                    AudioQuality.Higher => "exhigh",
                    AudioQuality.Lossless => "lossless",
                    AudioQuality.HiRes => "hires",
                    AudioQuality.Spatial => "jyeffect",
                    AudioQuality.Surround => "sky",
                    AudioQuality.Master => "jymaster",
                    _ => "standard"
                },
                ["encodeType"] = "flac"
            };
            if (p.AudioQuality is AudioQuality.Surround)
            {
                body["immerseType"] = "c51";
            }
            return body;
        }
    );

    /// <summary>
    /// Gets audio of a track in specific quality.
    /// </summary>
    /// <param name="trackId">ID of the track.</param>
    /// <param name="audioQuality">Quality of the audio.</param>
    /// <returns>Details of the track's audio.</returns>
    public Task<AudioApiResponse> GetAudioAsync(ulong trackId, AudioQuality audioQuality) =>
        AudioApi.RequestAsync((trackId, audioQuality));
}