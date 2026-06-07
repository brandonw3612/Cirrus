namespace Cirrus.Playback.EventArgs;

public class PlaybackPositionChangedEventArgs : System.EventArgs
{
    public required (TimeSpan Current, TimeSpan Total)? NewPosition { get; init; }
}