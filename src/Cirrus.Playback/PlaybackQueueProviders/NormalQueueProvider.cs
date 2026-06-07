using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Cirrus.Base.Services;
using Cirrus.Models.Business.Playback;
using Cirrus.Playback.EventArgs;
using Cirrus.Playback.Extensions;
using Cirrus.Playback.Primitives;

namespace Cirrus.Playback.PlaybackQueueProviders;

/// <summary>
/// Normal playback queue provider.
/// </summary>
/// <typeparam name="TTrackIdentifier">Type of track identifier.</typeparam>
/// <remarks>Often used by a normal playback source, such as a playlist or an album.</remarks>
public sealed partial class NormalQueueProvider<TTrackIdentifier> : PlaybackQueueProvider<TTrackIdentifier>
    where TTrackIdentifier : notnull
{
    #region Private states

    /// <summary>
    /// Lock object for thread safety. Used in state related operations.
    /// </summary>
    private readonly Lock _lock = new();
    
    /// <summary>
    /// Queue of tracks maintained by this provider.
    /// </summary>
    private readonly List<IAudioTrack<TTrackIdentifier>> _queue;
    
    /// <summary>
    /// Index of current track in the playback sequence.
    /// </summary>
    private int _currentTrackIndex;
    
    /// <summary>
    /// Sequence of playback order, value of which is the index of the track in the queue.
    /// </summary>
    /// <remarks>
    ///     When in sequential mode, this sequence is always ascending;
    ///     when in shuffle mode, this sequence is a random permutation of the indices of the tracks in the queue.
    /// </remarks>
    private List<int> _trackIndexSequence;

    private readonly SynchronizationContext? _syncCtx;

    #endregion

    #region Bindable stateful properties

    public override IAudioTrack<TTrackIdentifier>? CurrentTrack
    {
        get
        {
            lock (_lock)
            {
                return _currentTrackIndex < 0 || _currentTrackIndex >= _queue.Count
                    ? null
                    : _queue[_trackIndexSequence[_currentTrackIndex]];
            }
        }
    }

    private PlaybackMode _currentPlaybackMode;
    public override PlaybackMode? CurrentPlaybackMode
    {
        get => _currentPlaybackMode;
        set
        {
            if (value is null || value.Value == _currentPlaybackMode) return;
            OnPlaybackModeChanged(_currentPlaybackMode, value.Value);
            _currentPlaybackMode = value.Value;
            if (ServicesProvider.GetService<UserPreferenceService>() is { } userPreferenceService)
                userPreferenceService.Playback.PlaybackMode = value.Value;
            OnPropertyChanged();
        }
    }

    public override ObservableCollection<IAudioTrack<TTrackIdentifier>> UpcomingTracks { get; } = new();
    
    public override event EventHandler<CurrentTrackChangedEventArgs<TTrackIdentifier>>? CurrentTrackChanged;

    #endregion

    /// <summary>
    /// Constructs a new <see cref="NormalQueueProvider{TTrackIdentifier}"/> instance.
    /// </summary>
    /// <param name="syncCtx">Current synchronization context.</param>
    /// <param name="audioTracks">Tracks to be added to the queue initially.</param>
    /// <param name="startingTrackIndex">
    ///     Index of the track to be played first. Defaults to 0, indicating the first track.
    /// </param>
    /// <param name="playbackMode">
    ///     Playback mode of the queue. Defaults to null, indicating getting the settings from user preferences.
    /// </param>
    public NormalQueueProvider(SynchronizationContext? syncCtx, IAudioTrack<TTrackIdentifier>[] audioTracks, int startingTrackIndex = -1,
        PlaybackMode? playbackMode = null)
    {
        _syncCtx = syncCtx;
        _queue = audioTracks.ToList();
        _currentPlaybackMode = playbackMode ??
                               ServicesProvider.GetService<UserPreferenceService>()?.Playback.PlaybackMode ??
                               PlaybackMode.Sequential;
        if (_queue.Count is 0)
        {
            _trackIndexSequence = new();
            _currentTrackIndex = -1;
            return;
        }
        if (startingTrackIndex is -1)
        {
            startingTrackIndex = _currentPlaybackMode is PlaybackMode.Shuffle
                ? Random.Shared.Next(_queue.Count)
                : 0;
        }
        // Generate the track index sequence.
        _trackIndexSequence = _currentPlaybackMode is PlaybackMode.Shuffle
            ? GenerateShuffledSequence(_queue.Count)
            : Enumerable.Range(0, _queue.Count).ToList();
        _currentTrackIndex = _trackIndexSequence.IndexOf(startingTrackIndex);
    }

    internal override void InitiateState()
    {
        // Notify CurrentTrack changed.
        OnPropertyChanged(nameof(CurrentTrack));
        CurrentTrackChanged?.Invoke(this, new(CurrentTrack));
        // Generate UpcomingTracks.
        UpdateUpcomingTracks();
    }
    
    #region Supported public actions

    protected override PlaybackQueueActions SupportedActions => PlaybackQueueActions.None
        .EnableActions(PlaybackQueueActions.Previous)
        .EnableActions(PlaybackQueueActions.Next)
        .EnableActions(PlaybackQueueActions.SkipTo)
        .EnableActions(PlaybackQueueActions.Pend)
        .EnableActions(PlaybackQueueActions.Remove)
        .EnableActions(PlaybackQueueActions.Peek)
        .EnableActions(PlaybackQueueActions.SwitchPlaybackMode)
        .EnableActions(PlaybackQueueActions.Reset);

    public override Task<bool> PreviousAsync()
    {
        lock (_lock)
        {
            // Current track is undetermined or queue is empty. Regarded as invalid.
            if (_currentTrackIndex < 0 || _queue.Count is 0) return Task.FromResult(false);
            // Current track is at front of the queue and playback mode is 'sequential'.
            // We cannot go previous, but still handle this request as completed.
            if (_currentTrackIndex is not 0 || CurrentPlaybackMode is not PlaybackMode.Sequential)
            {
                _currentTrackIndex = (_currentTrackIndex + _queue.Count - 1) % _queue.Count;
            }
            // Notify CurrentTrack changed.
            OnPropertyChanged(nameof(CurrentTrack));
            CurrentTrackChanged?.Invoke(this, new(CurrentTrack));
            // Update UpcomingTracks.
            UpdateUpcomingTracks();
            return Task.FromResult(true);
        }
    }

    public override Task<bool> NextAsync()
    {
        lock (_lock)
        {
            // Current track is undetermined or queue is empty. Regarded as invalid.
            if (_currentTrackIndex < 0 || _queue.Count is 0) return Task.FromResult(false);
            // Current track is at end of the queue and playback mode is 'sequential'.
            // We cannot go next, and still handle this request as failed.
            if (_currentTrackIndex == _queue.Count - 1 && CurrentPlaybackMode is PlaybackMode.Sequential)
            {
                return Task.FromResult(false);
            }
            _currentTrackIndex = (_currentTrackIndex + 1) % _queue.Count;
            // Notify CurrentTrack changed.
            OnPropertyChanged(nameof(CurrentTrack));
            CurrentTrackChanged?.Invoke(this, new(CurrentTrack));
            // Update UpcomingTracks.
            UpdateUpcomingTracks();
            return Task.FromResult(true);
        }
    }

    public override Task<bool> SkipToAsync(TTrackIdentifier trackId)
    {
        lock (_lock)
        {
            // Current track is undetermined or queue is empty. Regarded as invalid.
            if (_currentTrackIndex < 0 || _queue.Count is 0) return Task.FromResult(false);
            var index = _queue.FindIndex(t => EqualityComparer<TTrackIdentifier>.Default.Equals(trackId, t.TrackId));
            // Track not found.
            if (index is -1) return Task.FromResult(false);
            _currentTrackIndex = _trackIndexSequence.IndexOf(index);
            // Notify CurrentTrack changed.
            OnPropertyChanged(nameof(CurrentTrack));
            CurrentTrackChanged?.Invoke(this, new(CurrentTrack));
            // Update UpcomingTracks.
            UpdateUpcomingTracks();
            return Task.FromResult(true);
        }
    }

    public override Task<bool> PrependAsync(IAudioTrack<TTrackIdentifier> track)
    {
        lock (_lock)
        {
            // Track already exists in the queue. Regarded as invalid.
            if (_queue.Any(t => EqualityComparer<TTrackIdentifier>.Default.Equals(t.TrackId, track.TrackId)))
                return Task.FromResult(false);
            // Insert the track right after current track, both in the queue and in playback sequence.
            var newTrackIndex = _trackIndexSequence[_currentTrackIndex] + 1;
            _queue.Insert(newTrackIndex, track);
            // Update the track index sequence.
            for (var i = 0; i < _trackIndexSequence.Count; i++)
            {
                if (_trackIndexSequence[i] >= newTrackIndex)
                {
                    _trackIndexSequence[i]++;
                }
            }
            // Insert index of the new track after modifying original indices to avoid conflict.
            _trackIndexSequence.Insert(_currentTrackIndex + 1, newTrackIndex);
            if (_currentTrackIndex is -1)
            {
                _currentTrackIndex = 0;
                // Notify CurrentTrack changed.
                OnPropertyChanged(nameof(CurrentTrack));
                CurrentTrackChanged?.Invoke(this, new(CurrentTrack));
            }
            // Update UpcomingTracks.
            UpdateUpcomingTracks();
            return Task.FromResult(true);
        }
    }

    public override Task<bool?> PrependAsync(IEnumerable<IAudioTrack<TTrackIdentifier>> tracks)
    {
        lock (_lock)
        {
            List<IAudioTrack<TTrackIdentifier>> tracksToPrepend = new();
            int existingTrackCount = 0;
            foreach (var track in tracks)
            {
                if (_queue.Any(t => EqualityComparer<TTrackIdentifier>.Default.Equals(t.TrackId, track.TrackId)))
                {
                    existingTrackCount++;
                    continue;
                }
                tracksToPrepend.Add(track);
            }
            // No tracks can be prepended. Regarded as invalid.
            if (tracksToPrepend.Count is 0) return Task.FromResult<bool?>(false);
            // Insert the tracks right after current track, both in the queue and in playback sequence.
            var newTrackIndex = _trackIndexSequence[_currentTrackIndex] + 1;
            _queue.InsertRange(newTrackIndex, tracksToPrepend);
            // Update the track index sequence.
            for (var i = 0; i < _trackIndexSequence.Count; i++)
            {
                if (_trackIndexSequence[i] >= newTrackIndex)
                {
                    _trackIndexSequence[i] += tracksToPrepend.Count;
                }
            }
            // Insert indices of the new tracks after modifying original indices to avoid conflict.
            _trackIndexSequence.InsertRange(_currentTrackIndex + 1,
                Enumerable.Range(newTrackIndex, tracksToPrepend.Count));
            if (_currentTrackIndex is -1)
            {
                _currentTrackIndex = 0;
                // Notify CurrentTrack changed.
                OnPropertyChanged(nameof(CurrentTrack));
                CurrentTrackChanged?.Invoke(this, new(CurrentTrack));
            }
            // Update UpcomingTracks.
            UpdateUpcomingTracks();
            // Return true if all tracks are prepended successfully; otherwise return null, indicating partial success.
            return Task.FromResult<bool?>(existingTrackCount is 0 ? true : null);
        }
    }

    public override Task<bool> AppendAsync(IAudioTrack<TTrackIdentifier> track)
    {
        lock (_lock)
        {
            // Track already exists in the queue. Regarded as failed.
            if (_queue.Any(t => EqualityComparer<TTrackIdentifier>.Default.Equals(t.TrackId, track.TrackId)))
                return Task.FromResult(false);
            // Insert the track to the end, both in the queue and in playback sequence.
            var newTrackIndex = _queue.Count;
            _queue.Add(track);
            _trackIndexSequence.Add(newTrackIndex);
            if (_currentTrackIndex is -1)
            {
                _currentTrackIndex = 0;
                // Notify CurrentTrack changed.
                OnPropertyChanged(nameof(CurrentTrack));
                CurrentTrackChanged?.Invoke(this, new(CurrentTrack));
            }
            // Update UpcomingTracks.
            UpdateUpcomingTracks();
            return Task.FromResult(true);
        }
    }

    public override Task<bool?> AppendAsync(IEnumerable<IAudioTrack<TTrackIdentifier>> tracks)
    {
        lock (_lock)
        {
            List<IAudioTrack<TTrackIdentifier>> tracksToAppend = new();
            int existingTrackCount = 0;
            foreach (var track in tracks)
            {
                if (_queue.Any(t => EqualityComparer<TTrackIdentifier>.Default.Equals(t.TrackId, track.TrackId)))
                {
                    existingTrackCount++;
                    continue;
                }
                tracksToAppend.Add(track);
            }
            // No tracks can be appended. Regarded as failed.
            if (tracksToAppend.Count is 0) return Task.FromResult<bool?>(false);
            // Insert the tracks to the end, both in the queue and in playback sequence.
            var newTrackIndex = _queue.Count;
            _queue.AddRange(tracksToAppend);
            _trackIndexSequence.AddRange(Enumerable.Range(newTrackIndex, tracksToAppend.Count));
            if (_currentTrackIndex is -1)
            {
                _currentTrackIndex = 0;
                // Notify CurrentTrack changed.
                OnPropertyChanged(nameof(CurrentTrack));
                CurrentTrackChanged?.Invoke(this, new(CurrentTrack));
            }
            // Update UpcomingTracks.
            UpdateUpcomingTracks();
            // Return true if all tracks are appended successfully; otherwise return null, indicating partial success.
            return Task.FromResult<bool?>(existingTrackCount is 0 ? true : null);
        }
    }

    public override Task<bool> RemoveAsync(TTrackIdentifier trackId)
    {
        lock (_lock)
        {
            var index = _queue.FindIndex(t => EqualityComparer<TTrackIdentifier>.Default.Equals(trackId, t.TrackId));
            // Track not found or is current track.
            if (index is -1 || index == _trackIndexSequence[_currentTrackIndex]) return Task.FromResult(false);
            // Remove the track, both in the queue and in the playback sequence.
            _queue.RemoveAt(index);
            // Find the index of the track in the playback sequence.
            var sequenceIndex = _trackIndexSequence.IndexOf(index);
            // Update the current track index.
            if (_currentTrackIndex > sequenceIndex)
            {
                _currentTrackIndex--;
            }
            _trackIndexSequence.RemoveAt(sequenceIndex);
            // Update the track index sequence.
            for (var i = 0; i < _trackIndexSequence.Count; i++)
            {
                if (_trackIndexSequence[i] >= index)
                {
                    _trackIndexSequence[i]--;
                }
            }
            if (_trackIndexSequence[_currentTrackIndex] >= index)
            {
                _currentTrackIndex--;
                // Notify CurrentTrack changed.
                OnPropertyChanged(nameof(CurrentTrack));
                CurrentTrackChanged?.Invoke(this, new(CurrentTrack));
            }
            // Update UpcomingTracks.
            UpdateUpcomingTracks();
            return Task.FromResult(true);
        }
    }

    public override Task<bool?> RemoveAsync(IEnumerable<TTrackIdentifier> trackIds)
    {
        int irremovableTrackCount = 0, removedTrackCount = 0;
        lock (_lock)
        {
            var currentTrackId = _queue[_trackIndexSequence[_currentTrackIndex]].TrackId;
            foreach (var trackId in trackIds)
            {
                var index = _queue.FindIndex(t =>
                    EqualityComparer<TTrackIdentifier>.Default.Equals(trackId, t.TrackId));
                // Track not found or is current track.
                if (index is -1 || index == _trackIndexSequence[_currentTrackIndex])
                {
                    irremovableTrackCount++;
                    continue;
                }
                _queue.RemoveAt(index);
                // Only in shuffle mode we need to remove the index from the sequence. Otherwise, we will just generate a new sequence.
                if (CurrentPlaybackMode is PlaybackMode.Shuffle)
                {
                    _trackIndexSequence.Remove(index);
                }
                removedTrackCount++;
            }

            // No track has been removed. Regarded as failed.
            if (removedTrackCount is 0) return Task.FromResult<bool?>(false);
            var currentTrackUpdatedIndex = _queue.FindIndex(t =>
                EqualityComparer<TTrackIdentifier>.Default.Equals(currentTrackId, t.TrackId));
            // Update the track index sequence.
            if (CurrentPlaybackMode is PlaybackMode.Shuffle)
            {
                // In shuffle mode the process is relatively complex, so the method below is proposed.
                // With this method the time complexity is O(n) because we avoided brute sorting.
                var maxTrackIndex = _trackIndexSequence.Max();
                // First, we create a bucket at the size that can hold all remaining indices.
                // Larger indices have already been dropped.
                // Default value is -1, indicating that the index is dropped.
                var bucket = Enumerable.Range(-1, maxTrackIndex + 1).ToArray();
                // Second, we fill the bucket with positions of remaining indices. By doing this, we actually finished sorting.
                // Now the bucket can be explained as:
                //      a[i] = j, where
                //          i is the index of the track in the queue before removing;
                //          j is the new position of it in the shrunk shuffle sequence.
                for (var i = 0; i < _trackIndexSequence.Count; i++)
                {
                    bucket[_trackIndexSequence[i]] = i;
                }
                // Last, we rearrange the sequence.
                // After ignoring dropped indices, we can simply refill each position (so as to the value in the bucket)
                //      with a new sequence index, starting from 0.
                int rearrangedTrackCount = 0;
                foreach (var t in bucket)
                {
                    if (t is -1) continue;
                    _trackIndexSequence[t] = rearrangedTrackCount;
                    rearrangedTrackCount++;
                }
                _currentTrackIndex = _trackIndexSequence.IndexOf(currentTrackUpdatedIndex);
            }
            else
            {
                // Regenerate the sequence.
                _trackIndexSequence = Enumerable.Range(0, _queue.Count).ToList();
                _currentTrackIndex = currentTrackUpdatedIndex;
            }
            // Update UpcomingTracks.
            UpdateUpcomingTracks();
            // Return true if all tracks are removed successfully; otherwise return null, indicating partial success.
            return Task.FromResult<bool?>(irremovableTrackCount is 0 ? true : null);
        }
    }

    private void OnPlaybackModeChanged(PlaybackMode oldValue, PlaybackMode newValue)
    {
        lock (_lock)
        {
            // Do nothing if the queue is empty.
            if (_queue.Count is 0) return;
            var realCurrentTrackIndex = _trackIndexSequence[_currentTrackIndex];
            // Shuffle -> Ascending. A new ascending sequence is generated.
            if (oldValue is PlaybackMode.Shuffle)
                _trackIndexSequence = Enumerable.Range(0, _queue.Count).ToList();
            // Ascending -> Shuffle. A new random sequence is generated.
            if (newValue is PlaybackMode.Shuffle)
                _trackIndexSequence = GenerateShuffledSequence(_queue.Count);
            _currentTrackIndex = _trackIndexSequence.IndexOf(realCurrentTrackIndex);
            // Update UpcomingTracks.
            UpdateUpcomingTracks();
        }
    }

    public override Task ResetAsync()
    {
        lock (_lock)
        {
            // Queue is empty.
            if (_queue.Count is 0)
            {
                _currentTrackIndex = -1;
                _trackIndexSequence = new();
            }
            else
            {
                // Reset current track to the front.
                _currentTrackIndex = 0;
            }
            // Notify CurrentTrack changed.
            OnPropertyChanged(nameof(CurrentTrack));
            CurrentTrackChanged?.Invoke(this, new(CurrentTrack));
            // Update UpcomingTracks.
            UpdateUpcomingTracks();
            return Task.CompletedTask;
        }
    }

    #endregion

    internal override Task<bool> StepForwardAsync()
    {
        lock (_lock)
        {
            // Current track is undetermined or queue is empty. Regarded as invalid.
            if (_currentTrackIndex < 0 || _queue.Count is 0) return Task.FromResult(false);
            if (CurrentPlaybackMode is PlaybackMode.RepeatOne) return Task.FromResult(true);
            // Current track is at end of the queue and playback mode is 'sequential'.
            // We cannot go next, and still handle this request as failed.
            if (_currentTrackIndex == _queue.Count - 1 && CurrentPlaybackMode is PlaybackMode.Sequential)
            {
                return Task.FromResult(false);
            }
            _currentTrackIndex = (_currentTrackIndex + 1) % _queue.Count;
            // Update CurrentTrack.
            OnPropertyChanged(nameof(CurrentTrack));
            // Update UpcomingTracks.
            UpdateUpcomingTracks();
            return Task.FromResult(true);
        }
    }

    internal override Task<IAudioTrack<TTrackIdentifier>?> InternalPeekAsync()
    {
        lock (_lock)
        {
            IAudioTrack<TTrackIdentifier>? track = null;
            if (_currentTrackIndex >= 0 && _queue.Count is not 0)
            {
                track = CurrentPlaybackMode switch
                {
                    PlaybackMode.RepeatOne => _queue[_trackIndexSequence[_currentTrackIndex]],
                    PlaybackMode.Sequential when _currentTrackIndex < _queue.Count - 1 =>
                        _queue[_trackIndexSequence[_currentTrackIndex + 1]],
                    PlaybackMode.Sequential => null,
                    _ => _queue[_trackIndexSequence[(_currentTrackIndex + 1) % _queue.Count]]
                };
            }
            return Task.FromResult(track);
        }
    }

    /// <summary>
    /// Private helper method to update <see cref="UpcomingTracks"/> after queue changes.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
    private void UpdateUpcomingTracks()
    {
        var totalUpcomingTracksCount = _queue.Count - _currentTrackIndex - 1;
        // Clear UpcomingTracks if queue is empty, or we are at the end of the queue.
        if (_queue.Count is 0 || totalUpcomingTracksCount is 0)
        {
            _syncCtx?.Post(_ => UpcomingTracks.Clear(), null);
            return;
        }
        // Limit list size to the maximum count.
        var listSize = Math.Min(totalUpcomingTracksCount, UpcomingTracksMaxCount);
        // Compute the updated list.
        var updatedList = _trackIndexSequence
            .Skip(_currentTrackIndex + 1)
            .Take(listSize)
            .Select(i => _queue[i])
            .ToArray();
        // In our design, the UpcomingTracks list is a bindable observable collection.
        // Thus, when updating the list, we had better not to clear it and add all items back.
        // Instead, we should remove items that are no longer in the list, rearrange the list, and insert new items.
        // The algorithm below is used to perform such operations.
        // First, we except the updated list from UpcomingTracks and obtain a list of tracks to remove.
        //      Then the removal is performed, after which UpcomingTracks would be a subset of the updated list.
        // Second, we compute the intersection of the updated list and UpcomingTracks.
        //      From this we have the new order of the tracks in UpcomingTracks that is kept in this update.
        // Third, we rearrange UpcomingTracks with the intersection.
        //      The rearrangement is described in RearrangeWith method. For reference, you can check the docs there.
        // Last, we insert the tracks that are not in UpcomingTracks.
        _syncCtx?.Post(_ =>
        {
            var tracksToRemove =
            UpcomingTracks.Except(updatedList, DefaultAudioTrackEqualityComparer<TTrackIdentifier>.Instance).ToList();
            tracksToRemove.ForEach(t => UpcomingTracks.Remove(t));
            var intersection = updatedList
                .Intersect(UpcomingTracks, DefaultAudioTrackEqualityComparer<TTrackIdentifier>.Instance).ToList();
            UpcomingTracks.RearrangeWith(intersection, DefaultAudioTrackEqualityComparer<TTrackIdentifier>.Instance);
            int m = 0, n = 0;
            while (m < updatedList.Length)
            {
                if (n >= UpcomingTracks.Count ||
                    !DefaultAudioTrackEqualityComparer<TTrackIdentifier>.Instance.Equals(UpcomingTracks[n],
                        updatedList[m]))
                    UpcomingTracks.Insert(n, updatedList[m]);
                m++;
                n++;
            }
        }, null);
        // After all these operations, UpcomingTracks is updated.
        // Here we do not need to notify the change since the collection is observable.
    }

    #region Helpers

    /// <summary>
    /// Generates a random sequence of integers.
    /// </summary>
    /// <param name="length">Length of the sequence.</param>
    /// <returns>A random sequence of integers.</returns>
    private static List<int> GenerateShuffledSequence(int length)
    {
        // Generate a random sequence.
        var sequence = Enumerable.Range(0, length).ToList();
        // Knuth-Durstenfeld shuffle algorithm.
        // Reference: https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle#The_modern_algorithm
        var random = new Random();
        for (var i = sequence.Count - 1; i >= 1; i--)
        {
            var j = random.Next(i + 1);
            (sequence[i], sequence[j]) = (sequence[j], sequence[i]);
        }
        return sequence;
    }
    
    #endregion
}