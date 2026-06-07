using Windows.Media.Audio;
using Cirrus.Models.Business.Playback;
using Cirrus.Models.Shared.Track;

namespace Cirrus.Playback.PlaybackServices;

/// <summary>
/// Audio node for audio graph.
/// </summary>
/// <typeparam name="TTrackIdentifier">Type of track identifier.</typeparam>
public sealed partial class AudioNode<TTrackIdentifier> : IDisposable where TTrackIdentifier : notnull
{
    /// <summary>
    /// Track associated with the audio node.
    /// </summary>
    public required IAudioTrack<TTrackIdentifier> Track { get; init; }
    
    /// <summary>
    /// Audio quality of the node.
    /// </summary>
    public required AudioQuality AudioQuality { get; init; }

    /// <summary>
    /// Audio node reference in the audio graph.
    /// </summary>
    public required MediaSourceAudioInputNode GraphNode { get; init; }

    private bool _isAttached;
    /// <summary>
    /// Whether current node is attached to the graph.
    /// </summary>
    public bool IsAttached
    {
        get => _isAttached;
        set
        {
            _isAttached = value;
            if (value) LastAccessTime = DateTimeOffset.Now;
        }
    }

    public DateTimeOffset LastAccessTime { get; private set; } = DateTimeOffset.Now;

    public bool IsCrossfadable => Track.Duration.TotalMinutes > 1;

    public void Dispose()
    {
        GraphNode.MediaSource.Dispose();
        GraphNode.Dispose();
    }
}