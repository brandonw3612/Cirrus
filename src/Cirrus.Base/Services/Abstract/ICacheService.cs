using Cirrus.Models.Business.Album;
using Cirrus.Models.Business.Lyrics;
using Cirrus.Models.Business.Playback;
using Cirrus.Models.Business.Playlist;
using Cirrus.Models.Business.Track;
using Cirrus.Models.Shared.User;

namespace Cirrus.Base.Services.Abstract;

public interface ICacheService
{
    static ICacheService? GetService() => ServicesProvider.GetService<ICacheService>();
    
    ValueTask InitializeAsync(CancellationToken cancellationToken = default);
    
    Task<Dictionary<ulong, OnlineTrack>> GetOnlineTracksAsync(
        IReadOnlyList<ulong> trackIds,
        CancellationToken cancellationToken = default
    );
    
    Task SaveOnlineTracksAsync(
        IReadOnlyList<OnlineTrack> tracks,
        CancellationToken cancellationToken = default
    );
    
    Task<TrackLyrics?> GetLyricsAsync(
        ulong trackId,
        CancellationToken cancellationToken = default
    );

    Task SaveLyricsAsync(
        ulong trackId,
        TrackLyrics lyrics,
        CancellationToken cancellationToken = default
    );
    
    Task<Dictionary<ulong, OnlinePlaylist>> GetPlaylistsAsync(
        IReadOnlyList<ulong> playlistIds,
        CancellationToken cancellationToken = default
    );

    Task SavePlaylistsAsync(
        IReadOnlyList<OnlinePlaylist> playlists,
        CancellationToken cancellationToken = default
    );
    
    Task<Dictionary<ulong, OnlineAlbum>> GetAlbumsAsync(
        IReadOnlyList<ulong> albumIds,
        CancellationToken cancellationToken = default
    );
    
    Task SaveAlbumsAsync(
        IReadOnlyList<OnlineAlbum> albums,
        CancellationToken cancellationToken = default
    );
    
    Task<UserProfile?> GetPersonalProfileAsync(
        CancellationToken cancellationToken = default
    );
    
    Task SavePersonalProfileAsync(
        UserProfile profile,
        CancellationToken cancellationToken = default
    );
    
    IAsyncEnumerable<OnlinePlaylist> GetPersonalPlaylistsAsync(
        CancellationToken cancellationToken = default
    );
    
    Task SavePersonalPlaylistsAsync(
        IReadOnlyList<OnlinePlaylist> playlists,
        CancellationToken cancellationToken = default
    );
    
    IAsyncEnumerable<OnlineAlbumBrief> GetPersonalAlbumsAsync(
        CancellationToken cancellationToken = default
    );
    
    Task SavePersonalAlbumsAsync(
        IReadOnlyList<OnlineAlbumBrief> albums,
        CancellationToken cancellationToken = default
    );
    
    Task<(IAudioTrack<ulong>[] Queue, int Index)?> GetPlaybackQueueAsync(
        CancellationToken cancellationToken = default
    );
    
    Task SavePlaybackQueueAsync(
        IReadOnlyList<IAudioTrack<ulong>> queue,
        CancellationToken cancellationToken = default
    );
    
    Task SavePlaybackQueueIndexAsync(
        int index,
        CancellationToken cancellationToken = default
    );
    
    Task ClearPlaybackQueueAsync(
        CancellationToken cancellationToken = default
    );
}

public static class CacheServiceExtensions
{
    public static async Task<OnlineAlbum?> GetAlbumAsync(
        this ICacheService cacheService,
        ulong albumId,
        CancellationToken cancellationToken = default
    )
    {
        var albums = await cacheService.GetAlbumsAsync(new List<ulong> { albumId }, cancellationToken);
        return albums.GetValueOrDefault(albumId);
    }
}