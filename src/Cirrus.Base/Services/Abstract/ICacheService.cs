using Cirrus.Models.Business.Lyrics;
using Cirrus.Models.Business.Playlist;
using Cirrus.Models.Business.Track;

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
}