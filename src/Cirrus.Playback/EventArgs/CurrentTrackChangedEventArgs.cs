using Cirrus.Models.Business.Playback;

namespace Cirrus.Playback.EventArgs;

/// <summary>
/// Event arguments for <see cref="Primitives.PlaybackQueueProvider{TTrackIdentifier}.CurrentTrackChanged"/> event.
/// </summary>
/// <typeparam name="TTrackIdentifier"></typeparam>
public class CurrentTrackChangedEventArgs<TTrackIdentifier> : System.EventArgs where TTrackIdentifier : notnull
{
    /// <summary>
    /// Updated current track in the queue.
    /// </summary>
    public IAudioTrack<TTrackIdentifier>? CurrentTrack { get; }
    
    /// <summary>
    /// Constructs a new <see cref="CurrentTrackChangedEventArgs{TTrackIdentifier}"/> instance.
    /// </summary>
    /// <param name="currentTrack">Updated current track in the queue.</param>
    public CurrentTrackChangedEventArgs(IAudioTrack<TTrackIdentifier>? currentTrack)
    {
        CurrentTrack = currentTrack;
    }
}