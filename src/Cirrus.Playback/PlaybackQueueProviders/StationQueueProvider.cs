using Cirrus.Models.Business.Playback;
using Cirrus.Playback.EventArgs;
using Cirrus.Playback.Primitives;

namespace Cirrus.Playback.PlaybackQueueProviders;

/// <summary>
/// Queue provider which is able to refill tracks from the station automatically.
/// </summary>
/// <typeparam name="TTrackIdentifier">Type of track identifier.</typeparam>
public sealed partial class StationQueueProvider<TTrackIdentifier> : PlaybackQueueProvider<TTrackIdentifier>
    where TTrackIdentifier : notnull
{
    #region Private fields

    /// <summary>
    /// Queue of the upcoming tracks.
    /// </summary>
    private readonly Queue<IAudioTrack<TTrackIdentifier>> _queue = new();

    /// <summary>
    /// Predefined track provider, used to refill the queue.
    /// </summary>
    private readonly Func<Task<IEnumerable<IAudioTrack<TTrackIdentifier>>>> _stationTrackProvider;

    /// <summary>
    /// Backing field for <see cref="CurrentTrack"/>.
    /// </summary>
    private IAudioTrack<TTrackIdentifier>? _currentTrack;

    /// <summary>
    /// Semaphore for thread-safe access to the queue and current track.
    /// </summary>
    private readonly SemaphoreSlim _queueSemaphore = new(1);

    #endregion

    public override IAudioTrack<TTrackIdentifier>? CurrentTrack => _currentTrack;

    #region Supported actions

    protected override PlaybackQueueActions SupportedActions => PlaybackQueueActions.None
        .EnableActions(PlaybackQueueActions.Next)
        .EnableActions(PlaybackQueueActions.Reset);

    public override async Task<bool> NextAsync()
    {
        // The queue is empty, and we failed to refill the queue.
        if (!await RefillQueueAsync(true)) return false;
        await _queueSemaphore.WaitAsync();
        try
        {
            _currentTrack = _queue.Dequeue();
            // Notify CurrentTrack changed.
            OnPropertyChanged(nameof(CurrentTrack));
            CurrentTrackChanged?.Invoke(this, new(CurrentTrack));
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            _queueSemaphore.Release();
            // Try refilling the queue when empty.
            await RefillQueueAsync(true);
        }
    }

    public override async Task ResetAsync()
    {
        // The queue is empty, and we failed to refill the queue.
        if (!await RefillQueueAsync(true))
        {
            throw new InvalidOperationException("Track provider is not working.");
        }
        await _queueSemaphore.WaitAsync();
        try
        {
            _currentTrack ??= _queue.Dequeue();
            // Notify CurrentTrack changed.
            OnPropertyChanged(nameof(CurrentTrack));
            CurrentTrackChanged?.Invoke(this, new(CurrentTrack));
        }
        finally
        {
            _queueSemaphore.Release();
            // Try refilling the queue when empty.
            await RefillQueueAsync(true);
        }
    }

    #endregion

    public override event EventHandler<CurrentTrackChangedEventArgs<TTrackIdentifier>>? CurrentTrackChanged;

    internal override void InitiateState()
    {
        // Notify CurrentTrack changed.
        OnPropertyChanged(nameof(CurrentTrack));
        CurrentTrackChanged?.Invoke(this, new(CurrentTrack));
    }
    
    internal override async Task<bool> StepForwardAsync()
    {
        // The queue is empty, and we failed to refill the queue.
        if (!await RefillQueueAsync(true)) return false;
        await _queueSemaphore.WaitAsync();
        try
        {
            _currentTrack = _queue.Dequeue();
            // Update CurrentTrack.
            OnPropertyChanged(nameof(CurrentTrack));
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            _queueSemaphore.Release();
            // Try refilling the queue when empty.
            await RefillQueueAsync(true);
        }
    }

    internal override async Task<IAudioTrack<TTrackIdentifier>?> InternalPeekAsync()
    {
        // The queue is empty, and we failed to refill the queue.
        if (!await RefillQueueAsync(true)) return null;
        await _queueSemaphore.WaitAsync();
        try
        {
            return _queue.FirstOrDefault();
        }
        finally
        {
            _queueSemaphore.Release();
        }
    }
    
    /// <summary>
    /// Private constructor, for we need asynchronous initialization.
    /// </summary>
    /// <param name="stationTrackProvider">Predefined track provider, used to refill the queue.</param>
    private StationQueueProvider(Func<Task<IEnumerable<IAudioTrack<TTrackIdentifier>>>> stationTrackProvider)
    {
        _stationTrackProvider = stationTrackProvider;
    }

    /// <summary>
    /// Asynchronously create a new <see cref="StationQueueProvider{TTrackIdentifier}"/> with the specified track provider.
    /// </summary>
    /// <param name="stationTrackProvider">Predefined track provider, used to refill the queue.</param>
    /// <returns>A new <see cref="StationQueueProvider{TTrackIdentifier}"/>.</returns>
    /// <exception cref="Exception">Thrown when the track provider is not working.</exception>
    public static async Task<StationQueueProvider<TTrackIdentifier>> CreateAsync(
        Func<Task<IEnumerable<IAudioTrack<TTrackIdentifier>>>> stationTrackProvider)
    {
        var provider = new StationQueueProvider<TTrackIdentifier>(stationTrackProvider);
        // Refill the queue from the beginning.
        if (!await provider.RefillQueueAsync())
        {
            throw new Exception("Track provider is not working.");
        }
        // Dequeue a track to set the current track.
        await provider._queueSemaphore.WaitAsync();
        provider._currentTrack = provider._queue.Dequeue();
        // Notify CurrentTrack changed.
        provider.OnPropertyChanged(nameof(provider.CurrentTrack));
        provider.CurrentTrackChanged?.Invoke(provider, new(provider.CurrentTrack));
        provider._queueSemaphore.Release();
        // Refill the queue when empty.
        await provider.RefillQueueAsync(true);
        return provider;
    }

    /// <summary>
    /// Refill the queue from the track provider.
    /// </summary>
    /// <param name="checkEmpty">
    ///     True to check whether the queue is empty before refilling.
    ///     Default is false, which indicates that the queue will be refilled forcibly.
    /// </param>
    /// <returns>
    ///     True, if the queue is refilled successfully or the queue is not empty as we requested;
    ///     False, otherwise.
    /// </returns>
    private async Task<bool> RefillQueueAsync(bool checkEmpty = false)
    {
        await _queueSemaphore.WaitAsync();
        try
        {
            if (checkEmpty && _queue.Count is not 0) return true;
            var tracks = (await _stationTrackProvider.Invoke()).ToList();
            if (tracks is {Count: 0}) return false;
            tracks.ForEach(_queue.Enqueue);
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            _queueSemaphore.Release();
        }
    }
}