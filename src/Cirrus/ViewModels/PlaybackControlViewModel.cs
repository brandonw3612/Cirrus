using Cirrus.Base.Services;
using Cirrus.Extensions;
using Cirrus.LiveModels;
using Cirrus.Models.Business.Lyrics;
using Cirrus.Models.Business.Playback;
using Cirrus.Playback.EventArgs;
using Cirrus.Playback.Extensions;
using Cirrus.Playback.Primitives;
using Cirrus.Primitives;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using WinRT;

namespace Cirrus.ViewModels;

[GeneratedBindableCustomProperty([
    nameof(IsLyricsTranslationEnabled),
    nameof(LyricsBaseFontSize)
], [])]
public partial class PlaybackControlViewModel : ViewModel
{
    public override string ViewIdentifier => string.Empty;
    
    private readonly UserPreferenceService _preference = ServicesProvider.Current.GetService<UserPreferenceService>()!;
    private readonly IPlaybackService<ulong> _playbackService = ServicesProvider.Current.GetService<IPlaybackService<ulong>>()!;
    public PlaybackServiceBridge PlaybackServiceBridge { get; } = new();

    public TrackLyrics? CurrentTrackLyrics { get; private set; }
    public bool LyricsScrollerOccupied { get; private set; }
    
    public PlaybackControlViewMode ViewMode
    {
        get => _preference.Appearance.PlaybackControlViewMode;
        set => _preference.Appearance.PlaybackControlViewMode = value;
    }

    public int ViewModeIndex
    {
        get => (int)ViewMode;
        set 
        {
            if ((int)ViewMode == value) return;
            ViewMode = (PlaybackControlViewMode)value;
            OnPropertyChanged(nameof(ViewMode));
            OnPropertyChanged();
        }
    }

    public bool IsLyricsTranslationEnabled
    {
        get => _preference.Appearance.IsLyricsTranslationVisible;
        set
        {
            if (_preference.Appearance.IsLyricsTranslationVisible == value) return;
            _preference.Appearance.IsLyricsTranslationVisible = value;
            OnPropertyChanged();
        }
    }

    public double LyricsBaseFontSize => _preference.Appearance.LyricsBaseFontSize;

    [RelayCommand]
    private void DecreaseFontSize()
    {
        if (_preference.Appearance.LyricsBaseFontSize <= 16) return;
        _preference.Appearance.LyricsBaseFontSize -= 4;
        OnPropertyChanged(nameof(LyricsBaseFontSize));
    }

    [RelayCommand]
    private void IncreaseFontSize()
    {
        if (_preference.Appearance.LyricsBaseFontSize >= 40) return;
        _preference.Appearance.LyricsBaseFontSize += 4;
        OnPropertyChanged(nameof(LyricsBaseFontSize));
    }

    public override Task LoadDataAsync()
    {
        PlaybackServiceBridge.CurrentTrackChanged -= OnCurrentTrackChanged;
        PlaybackServiceBridge.CurrentTrackChanged += OnCurrentTrackChanged;
        PlaybackServiceBridge.PlaybackPositionChanged -= OnPlaybackPositionChanged;
        PlaybackServiceBridge.PlaybackPositionChanged += OnPlaybackPositionChanged;
        return Task.CompletedTask;
    }

    private void OnPlaybackPositionChanged(object? sender, PlaybackPositionChangedEventArgs e)
    {
        if (e.NewPosition is not { } newPosition) return;
        DispatcherQueue!.TryEnqueue(() =>
        {
            SyncLyrics?.Invoke(newPosition.Current, newPosition.Total);
        });
    }

    private async void OnCurrentTrackChanged(object? sender, CurrentTrackChangedEventArgs<ulong> e)
    {
        if (e.CurrentTrack is not { } track)
        {
            DispatcherQueue!.TryEnqueue(() =>
            {
                RenderLyrics?.Invoke(null);
            });
            return;
        }
        var resp = await Network.Client.Track.GetLyricsAsync(track.TrackId);
        DispatcherQueue!.TryEnqueue(() =>
        {
            var lyrics = resp.ToLocal(track.Duration);
            RenderLyrics?.Invoke(lyrics);
            CurrentTrackLyrics = lyrics;
        });
    }
    
    public Action<TrackLyrics?>? RenderLyrics { get; set; }
    
    public Action<TimeSpan, TimeSpan>? SyncLyrics { get; set; }

    public Action<bool>? ToggleLyricsPreview { get; set; }

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

    [RelayCommand]
    private void OnLyricsScrollerInteractionStarted()
    {
        LyricsScrollerOccupied = true;
        ToggleLyricsPreview?.Invoke(true);
    }

    [RelayCommand]
    private void OnLyricsScrollerInteractionEnded()
    {
        LyricsScrollerOccupied = false;
        ToggleLyricsPreview?.Invoke(false);
    }
}
