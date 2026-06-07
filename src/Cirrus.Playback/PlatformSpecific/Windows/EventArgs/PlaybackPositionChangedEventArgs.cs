using Cirrus.Playback.PlaybackServices;

namespace Cirrus.Playback.EventArgs;

public class PlaybackPositionChangedEventArgs<TTrackIdentifier> : System.EventArgs where TTrackIdentifier : notnull
{
    public required (TimeSpan Current, TimeSpan Total)? NewPosition { get; init; }
    public required (AudioNode<TTrackIdentifier>? Current, AudioNode<TTrackIdentifier>? Next) Nodes { get; init; }
}