using System.Collections.ObjectModel;
using Cirrus.Models.Business.Playback;
using Cirrus.Playback.EventArgs;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Cirrus.Playback.Primitives;

/// <summary>
/// Playback queue provider.
/// </summary>
/// <typeparam name="TTrackIdentifier">The type of the track identifier.</typeparam>
/// <remarks>
///     Any class that implements this interface must be thread-safe. 
///     A provider only exposes the actions it supports and bindable stateful properties.
///     All internal operations and special logics should be encapsulated.
/// </remarks>
public abstract class PlaybackQueueProvider<TTrackIdentifier> : ObservableObject, IDisposable where TTrackIdentifier : notnull
{
    /// <summary>
    /// Actions supported by the provider.
    /// </summary>
    /// <remarks>All actions declared in this field must be implemented.</remarks>
    protected abstract PlaybackQueueActions SupportedActions { get; }

    #region Supported actions

    /// <summary>
    /// Whether <see cref="PlaybackQueueActions.Previous"/> is supported.
    /// </summary>
    /// <remarks>
    ///     If this field is false and the user attempts to call <see cref="PreviousAsync"/>,
    ///     a <see cref="NotSupportedException"/> will be thrown.
    /// </remarks>
    public bool IsPreviousSupported => SupportedActions.HasFlag(PlaybackQueueActions.Previous);

    /// <summary>
    /// Whether <see cref="PlaybackQueueActions.Next"/> is supported.
    /// </summary>
    /// <remarks>
    ///     If this field is false and the user attempts to call <see cref="NextAsync"/>,
    ///     a <see cref="NotSupportedException"/> will be thrown.
    /// </remarks>
    public bool IsNextSupported => SupportedActions.HasFlag(PlaybackQueueActions.Next);

    /// <summary>
    /// Whether <see cref="PlaybackQueueActions.SkipTo"/> is supported.
    /// </summary>
    /// <remarks>
    ///     If this field is false and the user attempts to call <see cref="SkipToAsync"/>,
    ///     a <see cref="NotSupportedException"/> will be thrown.
    /// </remarks>
    public bool IsSkipToSupported => SupportedActions.HasFlag(PlaybackQueueActions.SkipTo);

    /// <summary>
    /// Whether <see cref="PlaybackQueueActions.Pend"/> is supported.
    /// </summary>
    /// <remarks>
    ///     If this field is false and the user attempts to call
    ///         <see cref="PrependAsync(IAudioTrack{TTrackIdentifier})"/> or
    ///         <see cref="PrependAsync(System.Collections.Generic.IEnumerable{IAudioTrack{TTrackIdentifier}})"/> or
    ///         <see cref="AppendAsync(IAudioTrack{TTrackIdentifier})"/> or
    ///         <see cref="AppendAsync(System.Collections.Generic.IEnumerable{IAudioTrack{TTrackIdentifier}})"/>,
    ///     a <see cref="NotSupportedException"/> will be thrown.
    /// </remarks>
    public bool IsPendSupported => SupportedActions.HasFlag(PlaybackQueueActions.Pend);

    /// <summary>
    /// Whether <see cref="PlaybackQueueActions.Remove"/> is supported.
    /// </summary>
    /// <remarks>
    ///     If this field is false and the user attempts to call
    ///         <see cref="RemoveAsync(TTrackIdentifier)"/> or
    ///         <see cref="RemoveAsync(System.Collections.Generic.IEnumerable{TTrackIdentifier})"/>,
    ///     a <see cref="NotSupportedException"/> will be thrown.
    /// </remarks>
    public bool IsRemoveSupported => SupportedActions.HasFlag(PlaybackQueueActions.Remove);

    /// <summary>
    /// Whether <see cref="PlaybackQueueActions.Peek"/> is supported.
    /// </summary>
    /// <remarks>
    ///     If this field is false and the user attempts to get <see cref="UpcomingTracks"/>,
    ///     null will be returned.
    /// </remarks>
    public bool IsPeekSupported => SupportedActions.HasFlag(PlaybackQueueActions.Peek);

    /// <summary>
    /// Whether <see cref="PlaybackQueueActions.SwitchPlaybackMode"/> is supported.
    /// </summary>
    /// <remarks>
    ///     If this field is false and the user attempts to set <see cref="CurrentPlaybackMode"/>,
    ///     null will be returned.
    /// </remarks>
    public bool IsSwitchPlaybackModeSupported => SupportedActions.HasFlag(PlaybackQueueActions.SwitchPlaybackMode);

    /// <summary>
    /// Whether <see cref="PlaybackQueueActions.Reset"/> is supported.
    /// </summary>
    /// <remarks>
    ///     If this field is false and the user attempts to call <see cref="ResetAsync"/>,
    ///     a <see cref="NotSupportedException"/> will be thrown.
    /// </remarks>
    public bool IsResetSupported => SupportedActions.HasFlag(PlaybackQueueActions.Reset);

    #endregion

    #region Actions

    /// <summary>
    /// Switches to the previous track.
    /// </summary>
    /// <returns>Whether the operation is successful.</returns>
    /// <exception cref="NotSupportedException">
    ///     Thrown when <see cref="PlaybackQueueActions.Previous"/> is not supported by current provider.
    /// </exception>
    public virtual Task<bool> PreviousAsync() => throw new NotSupportedException();

    /// <summary>
    /// Switches to the next track.
    /// </summary>
    /// <returns>Whether the operation is successful.</returns>
    /// <exception cref="NotSupportedException">
    ///     Thrown when <see cref="PlaybackQueueActions.Next"/> is not supported by current provider.
    /// </exception>
    public virtual Task<bool> NextAsync() => throw new NotSupportedException();
    
    /// <summary>
    /// Switches to the track with the specified ID.
    /// </summary>
    /// <param name="trackId">ID of the track to switch to.</param>
    /// <returns>Whether the operation is successful.</returns>
    /// <exception cref="NotSupportedException">
    ///     Thrown when <see cref="PlaybackQueueActions.SkipTo"/> is not supported by current provider.
    /// </exception>
    public virtual Task<bool> SkipToAsync(TTrackIdentifier trackId) => throw new NotSupportedException();
    
    /// <summary>
    /// Prepends the specified track to the playback queue.
    /// </summary>
    /// <param name="track">Track to prepend to the playback queue.</param>
    /// <returns>Whether the operation is successful.</returns>
    /// <exception cref="NotSupportedException">
    ///     Thrown when <see cref="PlaybackQueueActions.Pend"/> is not supported by current provider.
    /// </exception>
    public virtual Task<bool> PrependAsync(IAudioTrack<TTrackIdentifier> track) => throw new NotSupportedException();

    /// <summary>
    /// Prepends the specified tracks to the playback queue.
    /// </summary>
    /// <param name="tracks">Tracks to prepend to the playback queue.</param>
    /// <returns>Whether the operation is successful.</returns>
    /// <exception cref="NotSupportedException">
    ///     Thrown when <see cref="PlaybackQueueActions.Pend"/> is not supported by current provider.
    /// </exception>
    public virtual Task<bool?> PrependAsync(IEnumerable<IAudioTrack<TTrackIdentifier>> tracks) =>
        throw new NotSupportedException();

    /// <summary>
    /// Appends the specified track to the playback queue.
    /// </summary>
    /// <param name="track">Track to append to the playback queue.</param>
    /// <returns>Whether the operation is successful.</returns>
    /// <exception cref="NotSupportedException">
    ///     Thrown when <see cref="PlaybackQueueActions.Pend"/> is not supported by current provider.
    /// </exception>
    public virtual Task<bool> AppendAsync(IAudioTrack<TTrackIdentifier> track) => throw new NotSupportedException();

    /// <summary>
    /// Appends the specified tracks to the playback queue.
    /// </summary>
    /// <param name="tracks">Tracks to append to the playback queue.</param>
    /// <returns>Whether the operation is successful.</returns>
    /// <exception cref="NotSupportedException">
    ///     Thrown when <see cref="PlaybackQueueActions.Pend"/> is not supported by current provider.
    /// </exception>
    public virtual Task<bool?> AppendAsync(IEnumerable<IAudioTrack<TTrackIdentifier>> tracks) =>
        throw new NotSupportedException();

    /// <summary>
    /// Removes the specified track from the playback queue.
    /// </summary>
    /// <param name="trackId">ID of the track to remove from the playback queue.</param>
    /// <returns>Whether the operation is successful.</returns>
    /// <exception cref="NotSupportedException">
    ///     Thrown when <see cref="PlaybackQueueActions.Remove"/> is not supported by current provider.
    /// </exception>
    public virtual Task<bool> RemoveAsync(TTrackIdentifier trackId) => throw new NotSupportedException();
    
    /// <summary>
    /// Removes the specified tracks from the playback queue.
    /// </summary>
    /// <param name="trackIds">IDs of the tracks to remove from the playback queue.</param>
    /// <returns>Whether the operation is successful.</returns>
    /// <exception cref="NotSupportedException">
    ///     Thrown when <see cref="PlaybackQueueActions.Remove"/> is not supported by current provider.
    /// </exception>
    public virtual Task<bool?> RemoveAsync(IEnumerable<TTrackIdentifier> trackIds) => throw new NotSupportedException();
    
    /// <summary>
    /// Resets the playback queue to default state.
    /// </summary>
    /// <exception cref="NotSupportedException">
    ///     Thrown when <see cref="PlaybackQueueActions.Reset"/> is not supported by current provider.
    /// </exception>
    public virtual Task ResetAsync() => throw new NotSupportedException();

    #endregion

    #region Bindable stateful properties

    /// <summary>
    /// Current track in the playback queue.
    /// </summary>
    public abstract IAudioTrack<TTrackIdentifier>? CurrentTrack { get; }

    /// <summary>
    /// Current playback mode of the playback queue.
    /// </summary>
    /// <remarks>Null if <see cref="PlaybackQueueActions.SwitchPlaybackMode"/> is not supported.</remarks>
    public virtual PlaybackMode? CurrentPlaybackMode { get; set; }

    /// <summary>
    /// Internal limit for the capacity of the upcoming tracks the user can peek.
    /// </summary>
    protected const int UpcomingTracksMaxCount = 50;

    /// <summary>
    /// Upcoming tracks in the playback queue.
    /// </summary>
    /// <remarks>Null if <see cref="PlaybackQueueActions.Peek"/> is not supported.</remarks>
    public virtual ReadOnlyObservableCollection<IAudioTrack<TTrackIdentifier>>? UpcomingTracks => null;

    #endregion

    /// <summary>
    /// Event raised when the current track in the playback queue changes.
    /// </summary>
    public abstract event EventHandler<CurrentTrackChangedEventArgs<TTrackIdentifier>>? CurrentTrackChanged;

    internal abstract void InitiateState();
    
    /// <summary>
    /// Step the playback queue forward. Only accessible internally.
    /// </summary>
    /// <returns>Whether the operation is successful.</returns>
    /// <remarks>This method is excepted to be called only when current track reaches the end.</remarks>
    internal abstract Task<bool> StepForwardAsync();

    /// <summary>
    /// Peeks the upcoming track in the playback queue. Only accessible internally.
    /// </summary>
    /// <returns>The upcoming track in the playback queue.</returns>
    /// <remarks>
    ///     This method is designed only to be called inside the playback module internally
    ///         to support preloading-related features.
    /// </remarks>
    internal abstract Task<IAudioTrack<TTrackIdentifier>?> InternalPeekAsync();

    public abstract void Dispose();
}