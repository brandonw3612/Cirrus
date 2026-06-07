using Windows.Devices.Enumeration;
using Windows.Media.Audio;
using Windows.Media.Render;
using Cirrus.Playback.EventArgs;

namespace Cirrus.Playback.PlaybackServices;

internal sealed partial class AudioGraphController : IDisposable
{
    private readonly AudioDeviceOutputNode _outputNode;
    private readonly Timer _audioGraphMonitorTimer;
    private bool _isCrossfading;
    private TimeSpan? _currentCrossfadeLength;
    
    public AudioGraph AudioGraph { get; }
    public TimeSpan? CrossfadeLength { get; set; }

    public bool IsCrossfading
    {
        get => _isCrossfading;
        set
        {
            _isCrossfading = value;
            if (value) _currentCrossfadeLength = CrossfadeLength;
        }
    }
    public DualNodeFlow<ulong> DualNodeFlow { get; }
    
    public event EventHandler<PlaybackPositionChangedEventArgs<ulong>>? PlaybackPositionChanged;
    public event EventHandler<System.EventArgs>? TrackEnded;

    private AudioGraphController(AudioGraph audioGraph, AudioDeviceOutputNode outputNode, TimeSpan? crossfadeLength)
    {
        AudioGraph = audioGraph;
        _outputNode = outputNode;
        CrossfadeLength = crossfadeLength;

        // Create audio effect definitions.
        _outputNode.EffectDefinitions.Clear();
        AudioEqualizerEffectsHelper.BuildEqualizerEffectDefinitions(AudioGraph)
            .ForEach(_outputNode.EffectDefinitions.Add);

        DualNodeFlow = new(
            node =>
            {
                try
                {
                    node.GraphNode.Stop();
                    node.GraphNode.Seek(TimeSpan.Zero);
                    node.GraphNode.AddOutgoingConnection(_outputNode);
                    node.GraphNode.MediaSourceCompleted -= OnNodeCompleted;
                    node.GraphNode.MediaSourceCompleted += OnNodeCompleted;
                    node.IsAttached = true;
                }
                catch
                {
                    // Ignored.
                }
            },
            node =>
            {
                try
                {
                    node.GraphNode.RemoveOutgoingConnection(_outputNode);
                    node.GraphNode.MediaSourceCompleted -= OnNodeCompleted;
                    node.IsAttached = false;
                }
                catch
                {
                    // Ignored.
                }
            });
        _audioGraphMonitorTimer =
            new(OnGraphMonitorTimerElapsed, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        audioGraph.Start();
    }

    private async void OnNodeCompleted(MediaSourceAudioInputNode sender, object args)
    {
        IsCrossfading = false;
        await DualNodeFlow.FlowAsync();
        TrackEnded?.Invoke(this, System.EventArgs.Empty);
    }

    internal static async Task<AudioGraphController> CreateAsync
        (TimeSpan? crossfadeLength, DeviceInformation? outputDevice = null)
    {
        AudioGraphSettings settings = new(AudioRenderCategory.Media);
        if (outputDevice is not null)
        {
            settings.PrimaryRenderDevice = outputDevice;
        }
        var graphCreateResult = await AudioGraph.CreateAsync(settings);
        if (graphCreateResult.Status is not AudioGraphCreationStatus.Success) throw graphCreateResult.ExtendedError;
        var graph = graphCreateResult.Graph;
        var nodeCreateResult = await graph.CreateDeviceOutputNodeAsync();
        if (nodeCreateResult.Status is not AudioDeviceNodeCreationStatus.Success) throw nodeCreateResult.ExtendedError;
        var outputNode = nodeCreateResult.DeviceOutputNode;
        return new(graph, outputNode, crossfadeLength);
    }

    public void ApplyEqualizerEffects(string effectName, double[]? customEffectInput)
    {
        if (_outputNode.EffectDefinitions.OfType<EqualizerEffectDefinition>().ToList() is not
            { Count: 3 } equalizerEffectDefinitions) return;
        AudioEqualizerEffectsHelper.UpdateEffectGains(equalizerEffectDefinitions, effectName, customEffectInput);
    }

    public void SetMonitorStatus(bool isMonitoring)
    {
        if (isMonitoring) _audioGraphMonitorTimer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(50));
        else _audioGraphMonitorTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public void SetVolume(double volume)
    {
        _outputNode.OutgoingGain = volume;
    }

    public void Dispose()
    {
        _outputNode.Dispose();
        AudioGraph.Dispose();
    }

    private void OnGraphMonitorTimerElapsed(object? _)
    {
        var (current, next) = DualNodeFlow.Nodes;
        if (current is null)
        {
            PlaybackPositionChanged?.Invoke(this, new()
            {
                NewPosition = null,
                Nodes = (current, next)
            });
            return;
        }
        if (IsCrossfading && CrossfadeLength is not null && next is not null)
        {
            var nextTrackProgress =
                Math.Clamp((next.GraphNode.Position / _currentCrossfadeLength).GetValueOrDefault(1d), 0, 1);
            nextTrackProgress = 0.5d - Math.Cos(nextTrackProgress * Math.PI) / 2;
            next.GraphNode.OutgoingGain = nextTrackProgress;
            current.GraphNode.OutgoingGain = 1d - nextTrackProgress;
            PlaybackPositionChanged?.Invoke(this, new()
            {
                NewPosition = (next.GraphNode.Position, next.GraphNode.Duration),
                Nodes = (current, next)
            });
            return;
        }
        current.GraphNode.OutgoingGain = 1d;
        PlaybackPositionChanged?.Invoke(this, new()
        {
            NewPosition = (current.GraphNode.Position, current.GraphNode.Duration),
            Nodes = (current, next)
        });
    }
}