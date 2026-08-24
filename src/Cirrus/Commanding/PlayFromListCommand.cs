using Cirrus.Base.Services;
using Cirrus.Commanding.Primitives;
using Cirrus.Models.Business.Playback;
using Cirrus.Playback.PlaybackQueueProviders;
using Cirrus.Playback.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Cirrus.Commanding;

public sealed partial class PlayFromListCommand : CommandWrapper
{
    [ObservableProperty] public partial IList<IAudioTrack<ulong>>? Tracks { get; set; }

    partial void OnTracksChanged(IList<IAudioTrack<ulong>>? value)
    {
        InnerCommand!.NotifyCanExecuteChanged();
    }

    public PlayFromListCommand()
    {
        var playbackService = ServicesProvider.Current.GetService<IPlaybackService<ulong>>();
        InnerCommand = new RelayCommand<IAudioTrack<ulong>>(track =>
        {
            if (Tracks is null || playbackService is null) return;
            var match = track is null
                ? null
                : Tracks.FirstOrDefault(t => t.TrackId == track.TrackId);
            var startIndex = match is null ? -1 : Tracks.IndexOf(match);
            playbackService.QueueProvider?.Dispose();
            playbackService.QueueProvider = new NormalQueueProvider<ulong>(Tracks.ToArray(), startIndex);
        }, _ => Tracks is not null && playbackService is not null);
    }
}