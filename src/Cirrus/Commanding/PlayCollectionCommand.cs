using Cirrus.Base.Services;
using Cirrus.Commanding.Primitives;
using Cirrus.Extensions;
using Cirrus.Models.Abstract;
using Cirrus.Models.Abstract.Primitives;
using Cirrus.Models.Business.Playback;
using Cirrus.Network;
using Cirrus.Playback.PlaybackQueueProviders;
using Cirrus.Playback.Primitives;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

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
                if (ServicesProvider.Current.GetService<IPlaybackService<ulong>>() is not { } playbackService ||
                    playable is null) return;
                switch (playable)
                {
                    case IPlaylist playlist:
                        {
                            var playlistResponse = await Client.Playlist.GetDetailsAsync(playlist.PlaylistId);
                            var trackIds = playlistResponse.Playlist?.TrackIds.TakeAtMost(100).ToList();
                            if (trackIds is null) return;
                            var trackResponse = await Client.Track.GetDetailsAsync(trackIds);
                            var syncCtx = SynchronizationContext.Current;
                            playbackService.QueueProvider = new NormalQueueProvider<ulong>(
                                syncCtx,
                                trackResponse
                                    .Tracks
                                    .Select(static t => t.ToBusinessModel())
                                    .OfType<IAudioTrack<ulong>>()
                                    .ToArray()
                            );
                            break;
                        }
                    case ITopChart playbackRecord:
                        {
                            var recordResponse = await Client.User.GetPlaybackRecordAsync(playbackRecord.UserId);
                            var syncCtx = SynchronizationContext.Current;
                            playbackService.QueueProvider = new NormalQueueProvider<ulong>(
                                syncCtx,
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
                            var syncCtx = SynchronizationContext.Current;
                            playbackService.QueueProvider = new NormalQueueProvider<ulong>(
                                syncCtx,
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