namespace Cirrus.Models.Shared.Search;

/// <summary>
/// Target parameter of a search in API search/search and search/cloud-search.
/// </summary>
public enum SearchTarget
{
    /// <summary>
    /// Targeted to all sorts of resource.
    /// </summary>
    General = 1018,
    /// <summary>
    /// Targeted to albums.
    /// </summary>
    Album = 10,
    /// <summary>
    /// Targeted to artists.
    /// </summary>
    Artist = 100,
    /// <summary>
    /// Targeted to lyrics.
    /// </summary>
    Lyrics = 1006,
    /// <summary>
    /// Targeted to music videos.
    /// </summary>
    MusicVideo = 1004,
    /// <summary>
    /// Targeted to playlists.
    /// </summary>
    Playlist = 1000,
    /// <summary>
    /// Targeted to podcast channels.
    /// </summary>
    PodcastChannel = 1009,
    /// <summary>
    /// Targeted to tracks.
    /// </summary>
    Track = 1,
    /// <summary>
    /// Targeted to users.
    /// </summary>
    User = 1002,
    // Video search is not enabled.
    // Video = 1014,
}