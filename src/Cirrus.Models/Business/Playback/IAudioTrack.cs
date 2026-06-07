namespace Cirrus.Models.Business.Playback;

/// <summary>
/// Interface for an audio track, identified by a <typeparamref name="TTrackIdentifier"/>.
/// </summary>
/// <typeparam name="TTrackIdentifier">Non-nullable type of the identifier of the audio track.</typeparam>
public interface IAudioTrack<out TTrackIdentifier> where TTrackIdentifier : notnull
{
    /// <summary>
    /// ID of the track.
    /// </summary>
    TTrackIdentifier TrackId { get; }
    
    /// <summary>
    /// Title of the track used by the playback module.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Whether the track's lyrics is marked as explicit.
    /// </summary>
    bool IsExplicit { get; }
    
    /// <summary>
    /// Combined name of the artists used by the playback module.
    /// </summary>
    string DisplayArtist { get; }
    
    /// <summary>
    /// Name of the album used by the playback module.
    /// </summary>
    string DisplayAlbum { get; }
    
    /// <summary>
    /// Duration of the track.
    /// </summary>
    TimeSpan Duration { get; }
    
    /// <summary>
    /// URI of the album artwork.
    /// </summary>
    Uri? AlbumArtworkUri { get; }
}

/// <summary>
/// Default equality comparer for <see cref="IAudioTrack{TTrackIdentifier}"/>. 
/// </summary>
/// <typeparam name="TTrackIdentifier">Non-nullable type of the identifier of the audio track.</typeparam>
/// <remarks>Only ID of the track is considered for comparison.</remarks>
public class DefaultAudioTrackEqualityComparer<TTrackIdentifier> : IEqualityComparer<IAudioTrack<TTrackIdentifier>>
    where TTrackIdentifier : notnull
{
    private static DefaultAudioTrackEqualityComparer<TTrackIdentifier>? _instance;
    /// <summary>
    /// Singleton instance of <see cref="DefaultAudioTrackEqualityComparer{TTrackIdentifier}"/>.
    /// </summary>
    public static DefaultAudioTrackEqualityComparer<TTrackIdentifier> Instance => _instance ??= new();
    
    public bool Equals(IAudioTrack<TTrackIdentifier>? x, IAudioTrack<TTrackIdentifier>? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return EqualityComparer<TTrackIdentifier>.Default.Equals(x.TrackId, y.TrackId);
    }

    public int GetHashCode(IAudioTrack<TTrackIdentifier> obj)
    {
        return EqualityComparer<TTrackIdentifier>.Default.GetHashCode(obj.TrackId);
    }
}