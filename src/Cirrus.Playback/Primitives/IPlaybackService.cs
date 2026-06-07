using System.ComponentModel;
using Cirrus.Models.Shared.Track;

namespace Cirrus.Playback.Primitives;

/// <summary>
/// Playback service interface.
/// </summary>
/// <typeparam name="TTrackIdentifier">Non-nullable type of the identifier of the audio track.</typeparam>
public interface IPlaybackService<TTrackIdentifier> : INotifyPropertyChanged where TTrackIdentifier : notnull
{
    #region Persistent properties

    /// <summary>
    /// Volume of the player.
    /// </summary>
    double Volume { get; set; }

    /// <summary>
    /// Audio quality of the player.
    /// </summary>
    public AudioQuality AudioQuality { get; set; }
    
    /// <summary>
    /// Whether direct switch is enabled.
    /// </summary>
    public bool IsDirectSwitchEnabled { get; set; }
    
    /// <summary>
    /// Whether audio cross fade is enabled.
    /// </summary>
    public bool IsAudioCrossfadeEnabled { get; set; }
    
    /// <summary>
    /// Audio cross fade duration in seconds.
    /// </summary>
    public double AudioCrossfadeDuration { get; set; }

    /// <summary>
    /// Equalizer effect key of the player, indicating a specific preset or custom equalizer effect. 
    /// </summary>
    public string EqualizerEffectKey { get; set; }
    
    /// <summary>
    /// Custom equalizer effect of the player. Must be a <see cref="double"/> array of length 10.
    /// </summary>
    public double[]? CustomEqualizerEffect { get; set; }

    /// <summary>
    /// Audio output device ID of the player.
    /// </summary>
    public string? AudioOutputDeviceId { get; set; }

    #endregion

    #region Properties

    /// <summary>
    /// Current queue provider, which provides actions for the playback queue.
    /// </summary>
    /// <remarks>This field is expected to be implemented as bindable.</remarks>
    PlaybackQueueProvider<TTrackIdentifier>? QueueProvider { get; set; }

    bool? IsPlaying { get; }
    
    (TimeSpan Current, TimeSpan Total)? PlaybackPosition { get; }
    
    object? PlaybackSource { get; }
    
    #endregion
    
    Task InitializeAsync();

    Task PlayPauseAsync();

    Task SeekAsync(TimeSpan targetPosition);

    Task ResetAsync();
}