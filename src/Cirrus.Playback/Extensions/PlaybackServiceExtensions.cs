using Cirrus.Playback.Primitives;

namespace Cirrus.Playback.Extensions;

public static class PlaybackServiceExtensions
{
    public static async Task TryPreviousAsync<T>(this IPlaybackService<T> playbackService) where T : notnull
    {
        if (playbackService is { IsDirectSwitchEnabled: false, PlaybackPosition.Current.TotalSeconds: > 3 })
        {
            await playbackService.SeekAsync(TimeSpan.Zero);
            return;
        }
        if (playbackService.QueueProvider is not { IsPreviousSupported: true } queueProvider) return;
        await queueProvider.PreviousAsync();
        if (playbackService is { IsDirectSwitchEnabled: true, IsPlaying: false })
        {
            await playbackService.PlayPauseAsync();
        }
    }

    public static async Task TryNextAsync<T>(this IPlaybackService<T> playbackService) where T : notnull
    {
        if (playbackService.QueueProvider is not { IsNextSupported: true } queueProvider) return;
        var succeeded = await queueProvider.NextAsync();
        if (succeeded)
        {
            if (playbackService is { IsDirectSwitchEnabled: true, IsPlaying: false })
            {
                await playbackService.PlayPauseAsync();
            }
            return;
        }
        await playbackService.ResetAsync();
    }
}