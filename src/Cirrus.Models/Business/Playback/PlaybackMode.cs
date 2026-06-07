namespace Cirrus.Models.Business.Playback;

/// <summary>
/// Playback mode of the playback module.
/// </summary>
public enum PlaybackMode
{
    /// <summary>
    /// Plays songs in original order, and stops when reaching the end of the queue.
    /// </summary>
    Sequential = 0,
    /// <summary>
    /// Plays songs in original order, and repeats the whole queue.
    /// </summary>
    RepeatAll = 1,
    /// <summary>
    /// Plays the current song on repeat.
    /// </summary>
    RepeatOne = 2,
    /// <summary>
    /// Shuffles the queue, plays songs in random order, and repeats the whole queue.
    /// </summary>
    Shuffle = 3
}