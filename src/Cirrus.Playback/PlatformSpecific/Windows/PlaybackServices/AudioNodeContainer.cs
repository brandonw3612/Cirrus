using Windows.Media.Audio;
using Windows.Media.Core;
using Cirrus.Models.Business.Playback;
using Cirrus.Models.Shared.Track;
using Cirrus.Network;

namespace Cirrus.Playback.PlaybackServices;

/// <summary>
/// Container for audio nodes.
/// </summary>
public class AudioNodeContainer
{
    /// <summary>
    /// Audio nodes available in the container.
    /// </summary>
    private readonly List<AudioNode<ulong>> _nodes = [];
    
    /// <summary>
    /// Semaphore for access to the nodes.
    /// </summary>
    private readonly SemaphoreSlim _accessSemaphore = new(1);
    
    /// <summary>
    /// Maximum capacity of the container for unoccupied nodes.
    /// </summary>
    private readonly int _capacity;
    
    /// <summary>
    /// AudioGraph for creating audio nodes.
    /// </summary>
    private readonly AudioGraph _graph;
    
    /// <summary>
    /// Constructs an audio node container for the specified audio graph.
    /// </summary>
    /// <param name="graph">Audio graph associated with the container.</param>
    /// <param name="capacity">Maximum capacity of the container for unoccupied nodes.</param>
    public AudioNodeContainer(AudioGraph graph, int capacity)
    {
        _graph = graph;
        _capacity = Math.Max(capacity, 3);
    }

    /// <summary>
    /// Gets an audio node for the specified track.
    /// </summary>
    /// <param name="track">Track to get audio node for.</param>
    /// <param name="audioQuality">Audio quality of the audio node.</param>
    /// <returns>Audio node for the specified track; Null if failed.</returns>
    public async Task<AudioNode<ulong>?> GetAudioNode(IAudioTrack<ulong> track, AudioQuality audioQuality)
    {
        await _accessSemaphore.WaitAsync();
        try
        {
            // Find in the cache.
            if (_nodes.FirstOrDefault(n =>
                n.Track.TrackId == track.TrackId &&
                n.AudioQuality == audioQuality &&
                !n.IsAttached) is { } node)
            {
                node.GraphNode.Seek(TimeSpan.Zero);
                return node;
            }
            // Fetch from NetEase.
            var response = await Client.Track.GetAudioAsync(track.TrackId, audioQuality);
            // Failed to get a URL for the audio.
            if (response.Audios.SingleOrDefault() is not { AudioUrl: { } audioUrl }) return null;
            // Audio available.
            var mediaSource = MediaSource.CreateFromUri(new(audioUrl));
            var graphNodeCreateResult = await _graph.CreateMediaSourceAudioInputNodeAsync(mediaSource);
            // Failed to create the node.
            if (graphNodeCreateResult.Status is not MediaSourceAudioInputNodeCreationStatus.Success) return null;
            // Node created.
            AudioNode<ulong> newNode = new()
            {
                Track = track,
                AudioQuality = audioQuality,
                GraphNode = graphNodeCreateResult.Node
            };
            _nodes.Add(newNode);
            var detached = _nodes.Where(static p => !p.IsAttached).ToArray();
            // No need to remove.
            if (detached.Length <= _capacity) return newNode;
            var removeNodes = detached.OrderBy(static n => n.LastAccessTime).Take(detached.Length - _capacity).ToArray();
            try
            {
                foreach (var n in removeNodes)
                {
                    n.Dispose();
                    _nodes.Remove(n);
                }
            }
            catch
            {
                // Ignored.
            }
            return newNode;
        }
        catch
        {
            return null;
        }
        finally
        {
            _accessSemaphore.Release();
        }
    }
}