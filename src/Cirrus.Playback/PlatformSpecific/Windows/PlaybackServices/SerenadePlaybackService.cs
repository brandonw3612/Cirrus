using System.ComponentModel;
using Windows.Devices.Enumeration;
using Windows.Media;
using Windows.Media.Devices;
using Windows.Storage.Streams;
using Cirrus.Base.Services;
using Cirrus.Models.Business.Playback;
using Cirrus.Models.Shared.Track;
using Cirrus.Playback.EventArgs;
using Cirrus.Playback.Extensions;
using Cirrus.Playback.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Pulse.Throttlers;

namespace Cirrus.Playback.PlaybackServices;

public partial class SerenadePlaybackService : ObservableObject, IPlaybackService<ulong>
{
    #region Persistent fields

    public double Volume
    {
        get => _userPreferenceService.Playback.Volume;
        set
        {
            if (Math.Abs(_userPreferenceService.Playback.Volume - value) < 0.001d) return;
            _userPreferenceService.Playback.Volume = value;
            _audioGraphController?.SetVolume(value);
            OnPropertyChanged();
        }
    }

    public AudioQuality AudioQuality
    {
        get => _userPreferenceService.Playback.AudioQuality;
        set => _userPreferenceService.Playback.AudioQuality = value;
    }

    public bool IsDirectSwitchEnabled
    {
        get => _userPreferenceService.Playback.IsDirectSwitchEnabled;
        set => _userPreferenceService.Playback.IsDirectSwitchEnabled = value;
    }

    public bool IsAudioCrossfadeEnabled
    {
        get => _userPreferenceService.Playback.IsAudioCrossfadeEnabled;
        set
        {
            _userPreferenceService.Playback.IsAudioCrossfadeEnabled = value;
            if (_audioGraphController is null) return;
            _audioGraphController.CrossfadeLength = value ? TimeSpan.FromSeconds(AudioCrossfadeDuration) : null;
        }
    }

    public double AudioCrossfadeDuration
    {
        get => _userPreferenceService.Playback.AudioCrossfadeDuration;
        set
        {
            if (value < 3d || value > 12d) return;
            _userPreferenceService.Playback.AudioCrossfadeDuration = value;
            if (_audioGraphController is null) return;
            _audioGraphController.CrossfadeLength = IsAudioCrossfadeEnabled
                ? TimeSpan.FromSeconds(value)
                : null;
        }
    }

    public string EqualizerEffectKey
    {
        get => _userPreferenceService.Playback.EqualizerEffectKey;
        set
        {
            if (_userPreferenceService.Playback.EqualizerEffectKey == value) return;
            _userPreferenceService.Playback.EqualizerEffectKey = value;
            _audioGraphController?.ApplyEqualizerEffects(value,
                CustomEqualizerEffect ?? Enumerable.Repeat(0d, 10).ToArray());
            OnPropertyChanged();
        }
    }

    public double[]? CustomEqualizerEffect
    {
        get => _userPreferenceService.Playback.CustomEqualizerEffect;
        set
        {
            switch (_userPreferenceService.Playback.CustomEqualizerEffect is null)
            {
                case true when value is null:
                case false when value is not null && _userPreferenceService.Playback.CustomEqualizerEffect.SequenceEqual(value):
                    return;
            }
            _userPreferenceService.Playback.CustomEqualizerEffect = value;
            _audioGraphController?.ApplyEqualizerEffects(EqualizerEffectKey, value);
            OnPropertyChanged();
        }
    }

    public string? AudioOutputDeviceId
    {
        get => _userPreferenceService.Playback.AudioOutputDeviceId;
        set => _userPreferenceService.Playback.AudioOutputDeviceId = value;
    }

    #endregion

    [ObservableProperty] public partial PlaybackQueueProvider<ulong>? QueueProvider { get; set; }
    [ObservableProperty] public partial (TimeSpan Current, TimeSpan Total)? PlaybackPosition { get; set; }
    [ObservableProperty] public partial bool? IsPlaying { get; set; }
    [ObservableProperty] public partial object? PlaybackSource { get; set; }

    private readonly UserPreferenceService _userPreferenceService;
    private SystemMediaTransportControls? _systemMediaTransportControls;

    private bool _isInitialized;
    private AudioGraphController? _audioGraphController;
    private AudioNodeContainer? _audioNodeContainer;
    private ActionThrottler<PlaybackPositionChangedEventArgs<ulong>> _positionMonitorThrottler;
    private bool _crossfadeSuppressed;
    
    public SerenadePlaybackService(UserPreferenceService userPreferenceService)
    {
        _userPreferenceService = userPreferenceService;
        _positionMonitorThrottler = new()
        {
            IsInstantaneous = true,
            ActionInterval = TimeSpan.FromMilliseconds(100),
            Action = OnPositionUpdated
        };
    }

    public void InitializeSystemMediaTransportControls(Window window)
    {
        var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
        _systemMediaTransportControls = SystemMediaTransportControlsInterop.GetForWindow(windowHandle);
        _systemMediaTransportControls.IsEnabled = false;
        _systemMediaTransportControls.ButtonPressed += async (_, args) =>
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play: await PlayPauseAsync(); break;
                case SystemMediaTransportControlsButton.Pause: await PlayPauseAsync(); break;
                case SystemMediaTransportControlsButton.Previous: await this.TryPreviousAsync(); break;
                case SystemMediaTransportControlsButton.Next: await this.TryNextAsync(); break;
            }
        };
        _systemMediaTransportControls.PlaybackPositionChangeRequested += async (_, args) =>
        {
            await SeekAsync(args.RequestedPlaybackPosition);
        };
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        TimeSpan? crossfadeLength = IsAudioCrossfadeEnabled
            ? TimeSpan.FromSeconds(AudioCrossfadeDuration)
            : null;
        _audioGraphController = AudioOutputDeviceId is "Default" or null
            ? await AudioGraphController.CreateAsync(crossfadeLength)
            : await AudioGraphController.CreateAsync(crossfadeLength,
                await GetDeviceInformationById(AudioOutputDeviceId));
        _audioGraphController!.SetVolume(Volume);
        _audioGraphController!.ApplyEqualizerEffects(EqualizerEffectKey, CustomEqualizerEffect);
        _audioGraphController!.PlaybackPositionChanged += (_, args) => _positionMonitorThrottler.Invoke(args);
        _audioGraphController!.TrackEnded += OnTrackEnded;
        _audioNodeContainer = new(_audioGraphController!.AudioGraph, 5);
    }

    public async Task PlayPauseAsync()
    {
        if (IsPlaying is null) return;
        var (current, next) = _audioGraphController!.DualNodeFlow.Nodes;
        if (current is null) return;
        if (_audioGraphController!.IsCrossfading)
        {
            current.GraphNode.Stop();
            next!.GraphNode.Stop();
            _audioGraphController.SetMonitorStatus(false);
            await _audioGraphController.DualNodeFlow.FlowAsync();
            _audioGraphController.IsCrossfading = false;
            IsPlaying = false;
            return;
        }
        if (IsPlaying is true)
        {
            current.GraphNode.Stop();
            IsPlaying = false;
        }
        else
        {
            current.GraphNode.Start();
            IsPlaying = true;
        }
        _audioGraphController.SetMonitorStatus(IsPlaying!.Value);
    }

    public async Task SeekAsync(TimeSpan targetPosition)
    {
        var (current, next) = _audioGraphController!.DualNodeFlow.Nodes;
        // Force status fix.
        if (next is null)
        {
            _audioGraphController!.IsCrossfading = false;
        }
        if (_audioGraphController!.IsCrossfading)
        {
            if (targetPosition > next!.GraphNode.Duration) return;
            current!.GraphNode.Stop();
            next.GraphNode.Seek(targetPosition);
            await _audioGraphController.DualNodeFlow.FlowAsync();
            _audioGraphController.IsCrossfading = false;
            return;
        }
        if (current is null) return;
        if (targetPosition > current.GraphNode.Duration) return;
        current.GraphNode.Seek(targetPosition);
        _crossfadeSuppressed = (current.GraphNode.Duration - targetPosition).TotalSeconds < 20;
        if (IsPlaying is not true)
        {
            PlaybackPosition = (current.GraphNode.Position, current.GraphNode.Duration);
        }
    }

    partial void OnQueueProviderChanged(PlaybackQueueProvider<ulong>? oldValue, PlaybackQueueProvider<ulong>? newValue)
    {
        if (oldValue is not null)
        {
            oldValue.CurrentTrackChanged -= OnCurrentTrackChanged;
            oldValue.PropertyChanged -= OnQueueProviderPropertyChanged;
            if (oldValue is IDisposable disposable) disposable.Dispose();
            IsPlaying = false;
        }
        ApplyTransportControlsFunctioning(newValue);
        if (newValue is null) return;
        newValue.CurrentTrackChanged -= OnCurrentTrackChanged;
        newValue.CurrentTrackChanged += OnCurrentTrackChanged;
        newValue.PropertyChanged -= OnQueueProviderPropertyChanged;
        newValue.PropertyChanged += OnQueueProviderPropertyChanged;
        IsPlaying = true;
        newValue.InitiateState();
    }

    partial void OnIsPlayingChanged(bool? value)
    {
        _systemMediaTransportControls!.PlaybackStatus = value switch
        {
            true => MediaPlaybackStatus.Playing,
            false => MediaPlaybackStatus.Paused,
            _ => MediaPlaybackStatus.Stopped
        };
    }

    partial void OnPlaybackPositionChanged((TimeSpan Current, TimeSpan Total)? value)
    {
        _systemMediaTransportControls!.UpdateTimelineProperties(new()
        {
            StartTime = TimeSpan.Zero,
            MinSeekTime = TimeSpan.Zero,
            Position = value?.Current ?? TimeSpan.Zero,
            MaxSeekTime = value?.Total ?? TimeSpan.Zero,
            EndTime = value?.Total ?? TimeSpan.Zero,
        });
    }

    private void OnQueueProviderPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not PlaybackQueueProvider<ulong> queueProvider) return;
        if (e.PropertyName is nameof(queueProvider.CurrentTrack))
        {
            UpdateTransportControlsProperties(queueProvider.CurrentTrack);
        }
    }

    private void ApplyTransportControlsFunctioning(PlaybackQueueProvider<ulong>? queueProvider)
    {
        _systemMediaTransportControls!.IsEnabled = queueProvider is not null;
        if (queueProvider is null) return;
        _systemMediaTransportControls.IsPauseEnabled = true;
        _systemMediaTransportControls.IsPlayEnabled = true;
        _systemMediaTransportControls.IsNextEnabled = queueProvider.IsNextSupported;
        _systemMediaTransportControls.IsPreviousEnabled = queueProvider.IsPreviousSupported;
    }

    private void UpdateTransportControlsProperties(IAudioTrack<ulong>? track)
    {
        var updater = _systemMediaTransportControls!.DisplayUpdater; 
        updater.Type = MediaPlaybackType.Music;
        updater.MusicProperties.Title = track?.Title ?? string.Empty;
        updater.MusicProperties.AlbumTitle = track?.DisplayAlbum ?? string.Empty;
        updater.MusicProperties.Artist = track?.DisplayArtist ?? string.Empty;
        // Pass track data to HotLyric.
        updater.MusicProperties.Genres.Clear();
        if (track is not null) updater.MusicProperties.Genres.Add($"NCM-{track.TrackId}");
        Uri imageUri = track?.AlbumArtworkUri is not { } artworkUri ||
                       !artworkUri.Scheme.StartsWith("http")
            ? new("ms-appx:///Assets/Images/DefaultAlbumArtwork.png")
            : new($"{artworkUri}?param=200y200");
        updater.Thumbnail = RandomAccessStreamReference.CreateFromUri(imageUri);
        updater.Update();
    }
    
    private async void OnCurrentTrackChanged(object? sender, CurrentTrackChangedEventArgs<ulong> e)
    {
        var (current, next) = _audioGraphController!.DualNodeFlow.Nodes;
        _audioGraphController!.SetMonitorStatus(false);
        if (e.CurrentTrack is null)
        {
            current?.GraphNode.Stop();
            await _audioGraphController.DualNodeFlow.ResetAsync();
            IsPlaying = null;
            return;
        }
        _crossfadeSuppressed = false;
        var start = IsDirectSwitchEnabled || IsPlaying is true;
        if (e.CurrentTrack.TrackId == next?.Track.TrackId)
        {
            await _audioGraphController.DualNodeFlow.FlowAsync();
            next.GraphNode.Seek(TimeSpan.Zero);
            PlaybackPosition = (TimeSpan.Zero, next.GraphNode.Duration);
            if (start)
            {
                next.GraphNode.Start();
                IsPlaying = true;
                _audioGraphController!.SetMonitorStatus(true);
            }
            return;
        }
        await _audioGraphController!.DualNodeFlow.ResetAsync();
        await PrepareNextTrackAsync(e.CurrentTrack);
        // Failed to create node.
        if (_audioGraphController!.DualNodeFlow.Nodes.Current is not { } node) return;
        PlaybackPosition = (TimeSpan.Zero, node.GraphNode.Duration);
        if (start)
        {
            node.GraphNode.Start();
            IsPlaying = true;
            _audioGraphController!.SetMonitorStatus(true);
        }
    }
    
    private async void OnPositionUpdated(PlaybackPositionChangedEventArgs<ulong> args)
    {
        PlaybackPosition = args.NewPosition;        
        var (current, next) = args.Nodes;
        if (current is null) return;
        // Next track does not exist.
        if (await QueueProvider!.InternalPeekAsync() is not { } nextTrack) return;
        var position = args.NewPosition!.Value;
        var timeLeft = position.Total - position.Current;
        if (_audioGraphController!.IsCrossfading) return;
        // Prepare next track.
        if (timeLeft.TotalSeconds < 40d &&
            (next is null || nextTrack.TrackId != next.Track.TrackId))
        {
            await PrepareNextTrackAsync(nextTrack);
            return;
        }
        // Start crossfading.
        if (IsAudioCrossfadeEnabled &&
            !_crossfadeSuppressed &&
            current is { IsCrossfadable: true } &&
            next is { IsCrossfadable: true } &&
            next.Track.TrackId == nextTrack.TrackId &&
            timeLeft.TotalSeconds < AudioCrossfadeDuration)
        {
            next.GraphNode.Start();
            _audioGraphController!.IsCrossfading = true;
            await QueueProvider!.StepForwardAsync();
        }
    }

    private async void OnTrackEnded(object? _, System.EventArgs _1)
    {
        if (await QueueProvider!.InternalPeekAsync() is not { } nextTrack)
        {
            await ResetAsync();
            return;
        }
        if (_audioGraphController?.DualNodeFlow.Nodes is { Current: { } currentNode } &&
            nextTrack.TrackId == currentNode.Track.TrackId)
        {
            currentNode.GraphNode.Start();
            await QueueProvider.StepForwardAsync();
            return;
        }
        await PrepareNextTrackAsync(nextTrack);
        if (_audioGraphController?.DualNodeFlow.Nodes is not { Current: { } newNode } ||
            nextTrack.TrackId != newNode.Track.TrackId) return;
        newNode.GraphNode.Start();
        await QueueProvider.StepForwardAsync();
    }

    public async Task ResetAsync()
    {
        if (QueueProvider is null) return;
        IsPlaying = false;
        await QueueProvider.ResetAsync();
    }

    private int _preparingTrack;
    
    private async Task PrepareNextTrackAsync(IAudioTrack<ulong> track)
    {
        if (Interlocked.Exchange(ref _preparingTrack, 1) is not 0) return;
        try
        {
            if (await _audioNodeContainer!.GetAudioNode(track, AudioQuality) is { } node)
            {
                await _audioGraphController!.DualNodeFlow.EnterAsync(node);
            }
        }
        finally
        {
            Interlocked.Exchange(ref _preparingTrack, 0);
        }
    }
    
    private static async Task<DeviceInformation?> GetDeviceInformationById(string deviceId)
    {
        try
        {
            if (await DeviceInformation.FindAllAsync(MediaDevice.GetAudioRenderSelector()) is not
                { Count: > 0 }) return null;
            var device = await DeviceInformation.CreateFromIdAsync(deviceId);
            return device;
        }
        catch
        {
            return null;
        }
    }
}