using System.Collections.ObjectModel;
using Cirrus.Base.Services;
using Cirrus.Models.Business.Playback;
using Cirrus.Playback.EventArgs;
using Cirrus.Playback.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;

namespace Cirrus.LiveModels;

public partial class PlaybackServiceBridge : ObservableObject
{
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly IPlaybackService<ulong> _playbackService;
    private PlaybackQueueProvider<ulong>? _playbackQueueProvider;

    [ObservableProperty] public partial ObservableCollection<IAudioTrack<ulong>> UpcomingTracks { get; set; } = [];

    [ObservableProperty] public partial bool IsControlAvailable { get; set; }

    [ObservableProperty] public partial bool IsQueueAvailable { get; set; }

    [ObservableProperty] public partial bool IsPreviousAvailable { get; set; }

    [ObservableProperty] public partial bool IsNextAvailable { get; set; }

    [ObservableProperty] public partial bool IsPlaybackModeSwitchable { get; set; }

    [ObservableProperty]
    public partial Uri AlbumArtworkUri { get; set; } = new("ms-appx:///Assets/Images/DefaultAlbumArtwork.png");

    [ObservableProperty] public partial string TrackTitle { get; set; } = "Cirrus";

    [ObservableProperty] public partial bool IsExplicit { get; set; } = false;

    [ObservableProperty] public partial string Artists { get; set; } = "Dedicated to creators";

    [ObservableProperty] public partial bool? IsPlaying { get; set; }

    [ObservableProperty] public partial string CurrentPlaybackMode { get; set; } = nameof(PlaybackMode.Sequential);

    [ObservableProperty] public partial IAudioTrack<ulong>? CurrentTrack { get; set; }

    public double Volume
    {
        get => _playbackService.Volume;
        set
        {
            if (Math.Abs(value - _playbackService.Volume) < 1e-3) return;
            _playbackService.Volume = value;
        }
    }

    public string VolumeLevel => _playbackService.Volume switch
    {
        < 1d / 3d => "0",
        < 2d / 3d => "1",
        _ => "2"
    };


    private TimeSpan _playbackPosition = TimeSpan.Zero;

    public double CurrentPositionMilliseconds
    {
        get => _playbackPosition.TotalMilliseconds;
        set
        {
            if (Math.Abs(value - _playbackPosition.TotalMilliseconds) < 1e-3) return;
            Seek(value);
        }
    }

    [ObservableProperty] public partial double DurationMilliseconds { get; set; }

    public event EventHandler<CurrentTrackChangedEventArgs<ulong>>? CurrentTrackChanged;
    public event EventHandler<PlaybackPositionChangedEventArgs>? PlaybackPositionChanged;

    public PlaybackServiceBridge()
    {
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        _playbackService = ServicesProvider.Current.GetService<IPlaybackService<ulong>>()!;
        Volume = _playbackService.Volume;
        _playbackService.PropertyChanged -= OnPlaybackServicePropertyChanged;
        _playbackService.PropertyChanged += OnPlaybackServicePropertyChanged;
        _playbackQueueProvider = _playbackService.QueueProvider;
        IsQueueAvailable = _playbackQueueProvider is { IsPeekSupported: true };
        IsControlAvailable = _playbackQueueProvider is not null;
        if (_playbackQueueProvider is null) return;
        _playbackQueueProvider.PropertyChanged -= OnQueueProviderPropertyChanged;
        _playbackQueueProvider.PropertyChanged += OnQueueProviderPropertyChanged;
        IsPreviousAvailable = _playbackQueueProvider.IsPreviousSupported;
        IsNextAvailable = _playbackQueueProvider.IsNextSupported;
        IsPlaybackModeSwitchable = _playbackQueueProvider.IsSwitchPlaybackModeSupported;
        CurrentPlaybackMode = (_playbackQueueProvider.CurrentPlaybackMode ?? PlaybackMode.Sequential).ToString();
        LoadCurrentTrack();
    }

    private async void Seek(double timestamp)
    {
        await _playbackService.SeekAsync(TimeSpan.FromMilliseconds(timestamp));
    }

    private async void OnPlaybackServicePropertyChanged(object? sender,
        System.ComponentModel.PropertyChangedEventArgs e)
    {
        await _dispatcherQueue.EnqueueAsync(() =>
        {
            switch (e.PropertyName)
            {
                case nameof(IPlaybackService<ulong>.QueueProvider):
                {
                    _playbackQueueProvider?.PropertyChanged -= OnQueueProviderPropertyChanged;
                    _playbackQueueProvider?.CurrentTrackChanged -= OnCurrentTrackChanged;
                    _playbackQueueProvider = _playbackService.QueueProvider;
                    IsControlAvailable = _playbackQueueProvider is not null;
                    if (_playbackQueueProvider is not null)
                    {
                        _playbackQueueProvider.PropertyChanged -= OnQueueProviderPropertyChanged;
                        _playbackQueueProvider.PropertyChanged += OnQueueProviderPropertyChanged;
                        _playbackQueueProvider.CurrentTrackChanged -= OnCurrentTrackChanged;
                        _playbackQueueProvider.CurrentTrackChanged += OnCurrentTrackChanged;
                        UpcomingTracks = _playbackQueueProvider.IsPeekSupported
                            ? _playbackQueueProvider.UpcomingTracks ?? []
                            : [];
                        IsPreviousAvailable = _playbackQueueProvider.IsPreviousSupported;
                        IsNextAvailable = _playbackQueueProvider.IsNextSupported;
                        CurrentPlaybackMode = (_playbackQueueProvider.CurrentPlaybackMode ?? PlaybackMode.Sequential)
                            .ToString();
                        IsPlaybackModeSwitchable = _playbackQueueProvider.IsSwitchPlaybackModeSupported;
                        LoadCurrentTrack();
                    }

                    break;
                }
                case nameof(IPlaybackService<ulong>.IsPlaying):
                {
                    IsPlaying = _playbackService.IsPlaying;
                    break;
                }
                case nameof(IPlaybackService<ulong>.PlaybackPosition):
                {
                    _playbackPosition = _playbackService.PlaybackPosition?.Current ?? TimeSpan.Zero;
                    OnPropertyChanged(nameof(CurrentPositionMilliseconds));
                    DurationMilliseconds = _playbackService.PlaybackPosition?.Total.TotalMilliseconds ?? 0;
                    PlaybackPositionChanged?.Invoke(this, new()
                    {
                        NewPosition = _playbackService.PlaybackPosition
                    });
                    break;
                }
                case nameof(IPlaybackService<ulong>.Volume):
                {
                    OnPropertyChanged(nameof(Volume));
                    OnPropertyChanged(nameof(VolumeLevel));
                    break;
                }
            }
        });
    }

    private void OnCurrentTrackChanged(object? sender, CurrentTrackChangedEventArgs<ulong> e) =>
        CurrentTrackChanged?.Invoke(this, e);

    private async void OnQueueProviderPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        await _dispatcherQueue.EnqueueAsync(() =>
        {
            switch (e.PropertyName)
            {
                case nameof(PlaybackQueueProvider<ulong>.CurrentTrack):
                {
                    LoadCurrentTrack();
                    break;
                }
                case nameof(PlaybackQueueProvider<ulong>.CurrentPlaybackMode):
                {
                    CurrentPlaybackMode =
                        (_playbackQueueProvider?.CurrentPlaybackMode ?? PlaybackMode.Sequential).ToString();
                    break;
                }
            }
        });
    }

    private void LoadCurrentTrack()
    {
        CurrentTrack = _playbackQueueProvider?.CurrentTrack;
        AlbumArtworkUri = _playbackQueueProvider?.CurrentTrack?.AlbumArtworkUri ??
                          new("ms-appx:///Assets/Images/DefaultAlbumArtwork.svg");
        TrackTitle = _playbackQueueProvider?.CurrentTrack?.Title ?? "Unknown Track";
        IsExplicit = _playbackQueueProvider?.CurrentTrack?.IsExplicit ?? false;
        Artists = _playbackQueueProvider?.CurrentTrack?.DisplayArtist ?? "Unknown Artists";
        CurrentTrackChanged?.Invoke(this, new(_playbackQueueProvider?.CurrentTrack));
    }
}