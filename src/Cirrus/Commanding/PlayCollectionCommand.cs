using Cirrus.Base.Services;
using Cirrus.Base.Services.Abstract;
using Cirrus.Commanding.Primitives;
using Cirrus.Extensions;
using Cirrus.Models.Abstract;
using Cirrus.Models.Abstract.Primitives;
using Cirrus.Models.Business.Playback;
using Cirrus.Models.Business.Playlist;
using Cirrus.Models.Business.Track;
using Cirrus.Models.Network.Track;
using Cirrus.Network;
using Cirrus.Network.Exceptions;
using Cirrus.Playback.PlaybackQueueProviders;
using Cirrus.Playback.Primitives;
using CommunityToolkit.Mvvm.Input;

namespace Cirrus.Commanding;

public sealed partial class PlayCollectionCommand : CommandWrapper
{
    private static PlayCollectionCommand? _instance; 
    public static PlayCollectionCommand Instance => _instance ??= new();
    
    public PlayCollectionCommand()
    {
        InnerCommand = new RelayCommand<IPlayable>(
            async playable =>
            {
                if (ServicesProvider.GetService<IPlaybackService<ulong>>() is not { } playbackService ||
                    playable is null) return;
                switch (playable)
                {
                    case IPlaylist playlist:
                        {
                            var cacheService = ServicesProvider.GetService<ICacheService>();
                            List<ulong> trackIds = [];
                            bool playlistCacheHit = false;
                            if (cacheService is not null)
                            {
                                var cachedPlaylists =
                                    await cacheService.GetPlaylistsAsync(new List<ulong> { playlist.PlaylistId });
                                if (cachedPlaylists.TryGetValue(playlist.PlaylistId, out var cachedPlaylist))
                                {
                                    trackIds = cachedPlaylist.TrackIds;
                                    playlistCacheHit = true;
                                }
                            }
                            if (!playlistCacheHit)
                            {
                                var playlistResponse = await Client.Playlist.GetDetailsAsync(playlist.PlaylistId);
                                if (playlistResponse.Playlist is null) return;
                                if (cacheService != null)
                                    await cacheService.SavePlaylistsAsync(new List<OnlinePlaylist>
                                        { playlistResponse.Playlist.ToBusinessModel() });
                                if (playlistResponse.Playlist?.TrackIds is { } ids) trackIds = ids;
                            }
                            if (trackIds.Count == 0)
                            {
                                return;
                            }
                            var index = 0;
                            Dictionary<ulong, OnlineTrack> trackCache = [];
                            var cacheMiss = trackIds;
                            if (cacheService is not null)
                            {
                                trackCache = await cacheService.GetOnlineTracksAsync(trackIds);
                                cacheMiss = trackIds.Except(trackCache.Keys).TakeAtMost(100).ToList();
                            }
                            List<TrackDetail> fetchedTracks = []; 
                            if (cacheMiss.Count > 0)
                            {
                                try
                                {
                                    var trackResponse = await Client.Track.GetDetailsAsync(cacheMiss);
                                    if (trackResponse.Tracks is { } ts) fetchedTracks = ts;
                                }
                                catch (UnsuccessfulResponseException ex) when (ex.StatusCode is 404)
                                {
                                    // Ignored.
                                }
                            }
                            var fetchedTrackMap = fetchedTracks.ToDictionary(t => t.TrackId);
                            List<IAudioTrack<ulong>> queue = [];
                            List<OnlineTrack> tracksToCache = [];
                            while (index < trackIds.Count)
                            {
                                var trackId = trackIds[index];
                                if (trackCache.TryGetValue(trackId, out var cachedTrack))
                                {
                                    queue.Add(cachedTrack);
                                }
                                else if (fetchedTrackMap.TryGetValue(trackId, out var track))
                                {
                                    var local = track.ToBusinessModel();
                                    queue.Add(local);
                                    tracksToCache.Add(local);
                                }
                                else if (!cacheMiss.Contains(trackId))
                                {
                                    break;
                                }
                                index++;
                            }
                            playbackService.QueueProvider = new NormalQueueProvider<ulong>(queue.ToArray());
                            if (tracksToCache.Count > 0 &&
                                ServicesProvider.GetService<ICacheService>() is { } cs)
                            {
                                await cs.SaveOnlineTracksAsync(tracksToCache);
                            }
                            break;
                        }
                    case ITopChart playbackRecord:
                        {
                            var recordResponse = await Client.User.GetPlaybackRecordAsync(playbackRecord.UserId);
                            playbackService.QueueProvider?.Dispose();
                            playbackService.QueueProvider = new NormalQueueProvider<ulong>(
                                recordResponse
                                    .WeeklyTracks
                                    .Select(r => r.Track?.ToBusinessModel())
                                    .OfType<IAudioTrack<ulong>>()
                                    .ToArray()
                            );
                            break;
                        }
                    case IAlbum album:
                        {
                            var albumResponse = await Client.Album.GetDetailsAsync(album.AlbumId);
                            playbackService.QueueProvider?.Dispose();
                            playbackService.QueueProvider = new NormalQueueProvider<ulong>(
                                albumResponse
                                    .Tracks
                                    .Select(static t => t.ToBusinessModel())
                                    .OfType<IAudioTrack<ulong>>()
                                    .ToArray()
                            );
                            break;
                        }
                    default:
                        throw new NotSupportedException("Playable type not supported.");
                }
            },
            playable => playable is not null &&
                playable is not ITopChart { IsAccessible: false }
        );
    }
}