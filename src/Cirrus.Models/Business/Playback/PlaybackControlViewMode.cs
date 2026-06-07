namespace Cirrus.Models.Business.Playback;

/// <summary>
/// View mode for playback control view.
/// </summary>
public enum PlaybackControlViewMode
{
    /// <summary>
    /// Only displays album cover in the center.
    /// </summary>
    Cover = 0,
    /// <summary>
    /// Displays album cover on the left and lyrics on the right.
    /// </summary>
    Lyrics = 1,
    /// <summary>
    /// Displays album cover on the left and playback queue on the right.
    /// </summary>
    Queue = 2
}