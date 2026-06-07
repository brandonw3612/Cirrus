using Cirrus.Base.Services;
using Cirrus.LiveModels;
using Cirrus.Models.Business.Playback;
using Cirrus.Playback.Extensions;
using Cirrus.Playback.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Cirrus.ViewModels;

public partial class PlaybackControlBarViewModel : ObservableObject
{
    private readonly IPlaybackService<ulong> _playbackService = ServicesProvider.Current.GetService<IPlaybackService<ulong>>()!;
    public PlaybackServiceBridge PlaybackServiceBridge { get; } = new();
    
    [RelayCommand]
    private Task Previous() => _playbackService.TryPreviousAsync();

    [RelayCommand]
    private Task PlayPause() => _playbackService.PlayPauseAsync();

    [RelayCommand]
    private Task Next() => _playbackService.TryNextAsync();

    [RelayCommand]
    private void SwitchPlaybackMode()
    {
        if (_playbackService.QueueProvider is not { IsSwitchPlaybackModeSupported: true } queueProvider) return;
        queueProvider.CurrentPlaybackMode = queueProvider.CurrentPlaybackMode switch
        {
            PlaybackMode.Sequential => PlaybackMode.RepeatAll,
            PlaybackMode.RepeatAll => PlaybackMode.RepeatOne,
            PlaybackMode.RepeatOne => PlaybackMode.Shuffle,
            PlaybackMode.Shuffle => PlaybackMode.Sequential,
            _ => null
        };
    }
}