namespace Cirrus.Playback.Primitives;

/// <summary>
/// Actions that can be performed on the playback queue.
/// </summary>
[Flags]
public enum PlaybackQueueActions
{
    /// <summary>
    /// No action.
    /// </summary>
    None = 0,
    /// <summary>
    /// Switch to the previous track.
    /// </summary>
    Previous = 1 << 0,
    /// <summary>
    /// Switch to the next track.
    /// </summary>
    Next = 1 << 1,
    /// <summary>
    /// Skip to the specified track.
    /// </summary>
    SkipTo = 1 << 2,
    /// <summary>
    /// Prepend or append the specified track.
    /// </summary>
    Pend = 1 << 3,
    /// <summary>
    /// Remove the specified track.
    /// </summary>
    Remove = 1 << 4,
    /// <summary>
    /// Peek the upcoming tracks.
    /// </summary>
    Peek = 1 << 5,
    /// <summary>
    /// Switch the playback mode.
    /// </summary>
    SwitchPlaybackMode = 1 << 6,
    /// <summary>
    /// Reset the playback queue to default state.
    /// </summary>
    Reset = 1 << 7
}

/// <summary>
/// Extensions for <see cref="PlaybackQueueActions"/>.
/// </summary>
public static class PlaybackQueueActionsExtensions
{
    /// <summary>
    /// Enable the specified actions.
    /// </summary>
    /// <param name="current">Current actions.</param>
    /// <param name="actions">Actions to enable.</param>
    /// <returns>Actions with specified actions enabled.</returns>
    public static PlaybackQueueActions EnableActions(this PlaybackQueueActions current, PlaybackQueueActions actions) =>
        current | actions;
}