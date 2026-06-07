using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.User;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;
using PlaybackRecordApiParameter = (ulong UserId, bool? RecentWeek);

namespace Cirrus.Network.Api;

partial class User
{
    /// <summary>
    /// Gets playback record in the past week or all time for specific user.
    /// API Route: /api/user/playback-record.
    /// </summary>
    [MusicApi("playback-record")]
    internal MusicApi<PlaybackRecordApiParameter, UserPlaybackRecordApiResponse> PlaybackRecordApi => field ??= new(
        $"{RouteBase}/playback-record",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.RequestBase}/weapi/v1/play/record",
        p => new()
        {
            ["uid"] = p.UserId,
            ["type"] = p.RecentWeek is true ? 1 : 0
        }
    );

    /// <summary>
    /// Gets playback record in the past week or all time for specific user.
    /// </summary>
    /// <param name="userId">Id of the user.</param>
    /// <param name="recentWeek">
    /// True, for data based on recent week; <br/>
    /// False | null | default, for date of all time.
    /// </param>
    /// <returns>User's playback record list.</returns>
    /// <remarks>
    /// If the user has set the playback record as private, the ResponseCode will be -200.
    /// </remarks>
    public Task<UserPlaybackRecordApiResponse> GetPlaybackRecordAsync(ulong userId, bool? recentWeek = null) =>
        PlaybackRecordApi.RequestAsync((userId, recentWeek));
}