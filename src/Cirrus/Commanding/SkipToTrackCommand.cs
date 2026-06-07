using Cirrus.Base.Services;
using Cirrus.Commanding.Primitives;
using Cirrus.Models.Business.Playback;
using Cirrus.Playback.Primitives;
using CommunityToolkit.Mvvm.Input;

namespace Cirrus.Commanding;

public class SkipToTrackCommand : CommandWrapper
{
    private static SkipToTrackCommand? _instance;
    public static SkipToTrackCommand Instance => _instance ??= new();
    
    public SkipToTrackCommand()
    {
        InnerCommand = new AsyncRelayCommand<IAudioTrack<ulong>>(async track =>
        {
            if (track is null ||
                ServicesProvider.GetService<IPlaybackService<ulong>>() is not
                    { QueueProvider: { IsSkipToSupported: true } qp }) return;
            await qp.SkipToAsync(track.TrackId);
        });
    }
}