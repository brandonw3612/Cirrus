using Cirrus.Base.Services;
using Cirrus.Commanding.Primitives;
using Cirrus.Models.Business.Playback;
using Cirrus.Playback.PlaybackQueueProviders;
using Cirrus.Playback.Primitives;
using Cirrus.Views;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace Cirrus.Commanding;

public sealed partial class AppendTrackCommand : CommandWrapper
{
    private static AppendTrackCommand? _instance;
    public static AppendTrackCommand Instance => _instance ??= new();
    
    public AppendTrackCommand()
    {
        InnerCommand = new AsyncRelayCommand<IAudioTrack<ulong>>(static async track => {
            if (track is null || ServicesProvider.Current.GetService<IPlaybackService<ulong>>() is not
                    { } playbackService || MainWindow.Current is not { } window) return;
            if (playbackService.QueueProvider is null)
            {
                var syncCtx = SynchronizationContext.Current;
                playbackService.QueueProvider = new NormalQueueProvider<ulong>(syncCtx, [track]);
            }
            else if (playbackService.QueueProvider.IsPendSupported)
            {
                await playbackService.QueueProvider.AppendAsync(track);
            }
            else return;
            window.ShowNotification(
                "Notifications/TrackPended".GetLocalized() ?? "{InvalidResource}",
                1000,
                InfoBarSeverity.Success
            );
        });
    }
}