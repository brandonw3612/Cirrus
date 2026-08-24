using System.Collections.ObjectModel;
using System.ComponentModel;
using Cirrus.Models.Business.Playback;
using Cirrus.Playback.EventArgs;
using Cirrus.Playback.Primitives;

namespace Cirrus.Playback.PlaybackQueueProviders;

/// <summary>
/// Prefetched intelligent queue provider.
/// </summary>
/// <typeparam name="TTrackIdentifier">The type of the track identifier.</typeparam>
/// <remarks>
///     This class is implemented with <see cref="NormalQueueProvider{TTrackIdentifier}"/>,
///     by disabling some of its features.
/// </remarks>
public sealed partial class PrefetchedIntelligentQueueProvider<TTrackIdentifier> : PlaybackQueueProvider<TTrackIdentifier>, IDisposable
    where TTrackIdentifier : notnull
{
    private readonly NormalQueueProvider<TTrackIdentifier> _normalQueueProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrefetchedIntelligentQueueProvider{TTrackIdentifier}"/> class.
    /// </summary>
    /// <param name="audioTracks">Tracks to be added to the queue initially.</param>
    public PrefetchedIntelligentQueueProvider(IAudioTrack<TTrackIdentifier>[] audioTracks)
    {
        _normalQueueProvider = new(audioTracks, 0, PlaybackMode.RepeatAll);
        _normalQueueProvider.CurrentTrackChanged += OnCurrentTrackChanged;
        _normalQueueProvider.PropertyChanged += OnInnerPropertyChanged;
    }
    
    public override void Dispose()
    {
        _normalQueueProvider.CurrentTrackChanged -= OnCurrentTrackChanged;
        _normalQueueProvider.PropertyChanged -= OnInnerPropertyChanged;
        _normalQueueProvider.Dispose();
    }

    private void OnInnerPropertyChanged(object? _, PropertyChangedEventArgs args)
    {
        OnPropertyChanged(args.PropertyName);
    }

    private void OnCurrentTrackChanged(object? _, CurrentTrackChangedEventArgs<TTrackIdentifier> args)
    {
        CurrentTrackChanged?.Invoke(this, args);
    }

    #region Supported actions

    protected override PlaybackQueueActions SupportedActions => PlaybackQueueActions.None
        .EnableActions(PlaybackQueueActions.Previous)
        .EnableActions(PlaybackQueueActions.Next)
        .EnableActions(PlaybackQueueActions.SkipTo)
        .EnableActions(PlaybackQueueActions.Pend)
        .EnableActions(PlaybackQueueActions.Remove)
        .EnableActions(PlaybackQueueActions.Peek)
        .EnableActions(PlaybackQueueActions.Reset);

    public override Task<bool> PreviousAsync() => _normalQueueProvider.PreviousAsync();
    
    public override Task<bool> NextAsync() => _normalQueueProvider.NextAsync();

    public override Task<bool> SkipToAsync(TTrackIdentifier trackId) => _normalQueueProvider.SkipToAsync(trackId);

    public override Task<bool> PrependAsync(IAudioTrack<TTrackIdentifier> track) =>
        _normalQueueProvider.PrependAsync(track);

    public override Task<bool?> PrependAsync(IEnumerable<IAudioTrack<TTrackIdentifier>> tracks) =>
        _normalQueueProvider.PrependAsync(tracks);

    public override Task<bool> AppendAsync(IAudioTrack<TTrackIdentifier> track) =>
        _normalQueueProvider.AppendAsync(track);

    public override Task<bool?> AppendAsync(IEnumerable<IAudioTrack<TTrackIdentifier>> tracks) =>
        _normalQueueProvider.AppendAsync(tracks);

    public override Task<bool> RemoveAsync(TTrackIdentifier trackId) => _normalQueueProvider.RemoveAsync(trackId);

    public override Task<bool?> RemoveAsync(IEnumerable<TTrackIdentifier> trackIds) =>
        _normalQueueProvider.RemoveAsync(trackIds);

    public override Task ResetAsync() => _normalQueueProvider.ResetAsync();

    #endregion

    public override IAudioTrack<TTrackIdentifier>? CurrentTrack => _normalQueueProvider.CurrentTrack;

    public override ReadOnlyObservableCollection<IAudioTrack<TTrackIdentifier>> UpcomingTracks =>
        _normalQueueProvider.UpcomingTracks;

    public override event EventHandler<CurrentTrackChangedEventArgs<TTrackIdentifier>>? CurrentTrackChanged;

    internal override void InitiateState() => _normalQueueProvider.InitiateState();

    internal override Task<bool> StepForwardAsync() => _normalQueueProvider.StepForwardAsync();

    internal override Task<IAudioTrack<TTrackIdentifier>?> InternalPeekAsync() =>
        _normalQueueProvider.InternalPeekAsync();
}