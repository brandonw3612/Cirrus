using System.Reactive.Linq;
using Cirrus.Base.Services.Abstract;
using Cirrus.Constants;
using Cirrus.LiveModels;
using Cirrus.Models.Abstract.Primitives;
using Cirrus.Models.Business.Playlist;
using Cirrus.Models.Business.User;
using Cirrus.Models.Shared.User;
using Cirrus.Network;
using Cirrus.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using DynamicData;
using Microsoft.UI.Xaml;
using WinRT;

namespace Cirrus.ViewModels;

[GeneratedBindableCustomProperty([
    nameof(NavigatiableCardWidth)
], [])]
public partial class UserDetailViewModel : ViewModel<ulong>
{
    private readonly SourceList<INavigatiable>[] _playlistSources =
        [new(), new(), new()];
    private int[] _readyPlaylistCounts = [0, 0, 0];
    private readonly IDisposable[] _playlistSubscriptions;

    public override string ViewIdentifier => $"User{Parameter}";

    [ObservableProperty] public partial UserProfile? Profile { get; private set; }
    [ObservableProperty] public partial double NavigatiableCardWidth { get; private set; }

    public NavigatiableGroup[] Playlists { get; } =
    [
        new([])
        {
            GroupHeader = "Views/UserDetailView/List/MusicTaste/Header".GetLocalized() ?? "{Invalid Resource}"
        },
        new([])
        {
            GroupHeader = "Views/UserDetailView/List/ContributedPlaylists/Header".GetLocalized() ?? "{Invalid Resource}"
        },
        new([])
        {
            GroupHeader = "Views/UserDetailView/List/SubscribedPlaylists/Header".GetLocalized() ?? "{Invalid Resource}"
        }
    ];

    public UserDetailViewModel()
    {
        var synchronizationContext = ISynchronizationContextService.GetService()?.Get()
            ?? SynchronizationContext.Current
            ?? new SynchronizationContext();
        _playlistSubscriptions =
        [
            .._playlistSources.Select((source, index) => source
                .Connect()
                .ObserveOn(synchronizationContext)
                .Bind(Playlists[index], 10000)
                .Subscribe())
        ];
    }

    public override async Task LoadDataAsync()
    {
        var isPersonalProfile = Application.Current is App
            {
                CurrentAccount.UserAccount: { } currentAccount
            } && Parameter == currentAccount.UserId;
        var cacheService = ICacheService.GetService();

        if (isPersonalProfile && cacheService is not null &&
            await cacheService.GetPersonalProfileAsync() is { } profile)
        {
            Profile = profile;
            foreach (var source in _playlistSources) source.Clear();
            _playlistSources[0].Add(new TopChart
            {
                UserId = Parameter,
                Nickname = profile.Nickname,
                IsAccessible = true,
                PlayedTrackCount = profile.TrackCount
            });
            _readyPlaylistCounts = [1, 0, 0];
            await foreach (var playlist in cacheService.GetPersonalPlaylistsAsync())
            {
                AddPlaylist(playlist);
            }
        }

        var detailsResponse = await Client.User.GetDetailsAsync(Parameter);
        if (detailsResponse is not { Profile: { } newProfile }) return;
        Profile = newProfile;
        if (isPersonalProfile && cacheService is not null)
            await cacheService.SavePersonalProfileAsync(newProfile);
        
        if (_playlistSources[0].Items.FirstOrDefault() is TopChart topChart)
        {
            topChart.UserId = newProfile.UserId;
            topChart.Nickname = newProfile.Nickname;
            topChart.IsAccessible = detailsResponse.IsPlaybackRecordPublic || isPersonalProfile;
            topChart.PlayedTrackCount = newProfile.TrackCount;
        }
        else
        {
            _playlistSources[0].Edit(inner =>
            {
                var topCharts = inner.OfType<TopChart>().ToList();
                topCharts.ForEach(c => inner.Remove(c));
                inner.Insert(0, new TopChart
                {
                    UserId = newProfile.UserId,
                    Nickname = newProfile.Nickname,
                    IsAccessible = detailsResponse.IsPlaybackRecordPublic || isPersonalProfile,
                    PlayedTrackCount = newProfile.TrackCount
                });
            });
        }
        _readyPlaylistCounts = [1, 0, 0];

        List<OnlinePlaylist> toCache = new();
        var playlistCount = 0;
        while (true)
        {
            var pm = Enumerable.Range(0, 3).ToDictionary(i => i, _ => new List<OnlinePlaylist>());
            var playlistsResponse = await Client.User.GetPlaylistsAsync(Parameter, playlistCount);
            foreach (var playlist in playlistsResponse.Playlists)
            {
                var onlinePlaylist = playlist.ToBusinessModel();
                toCache.Add(onlinePlaylist);
                if (onlinePlaylist.CreatorId == Parameter &&
                    onlinePlaylist is { Type: PlaylistType.Likelist or PlaylistType.Annual })
                {
                    pm[0].Add(onlinePlaylist);
                }
                else if (onlinePlaylist.CreatorId == Parameter ||
                         onlinePlaylist.Type == PlaylistType.Shared &&
                         onlinePlaylist.SharedCreators.Any(c => c.UserId == Parameter))
                {
                    pm[1].Add(onlinePlaylist);
                }
                else
                {
                    pm[2].Add(onlinePlaylist);
                }
                playlistCount++;
            }
            for (int i = 0; i <= 2; i++)
            {
                var ppm = pm;
                var ii = i;
                _playlistSources[i].Edit(inner =>
                {
                    for (int j = 0; j <= 2; j++)
                    {
                        if (j == ii) continue;
                        var toRemove = inner.OfType<OnlinePlaylist>()
                            .Intersect(ppm[j], OnlinePlaylistIdComparer.Default)
                            .ToList();
                        toRemove.ForEach(p => inner.Remove(p));
                    }
                    for (int k = 0; k < ppm[ii].Count; k++)
                    {
                        var matches = inner
                            .Skip(_readyPlaylistCounts[ii])
                            .Where(p => p is OnlinePlaylist ep && ep.PlaylistId == ppm[ii][k].PlaylistId)
                            .ToList();
                        switch (matches.Count)
                        {
                            case 0:
                                inner.Insert(_readyPlaylistCounts[ii], ppm[ii][k]);
                                break;
                            case 1:
                                if (inner[_readyPlaylistCounts[ii]] is OnlinePlaylist ep &&
                                    ep.PlaylistId == ppm[ii][k].PlaylistId &&
                                    ep.UpdatedTime == ppm[ii][k].UpdatedTime)
                                {
                                    break;
                                }
                                inner.Remove(matches.Single());
                                inner.Insert(_readyPlaylistCounts[ii], ppm[ii][k]);
                                break;
                            default:
                                matches.ForEach(p => inner.Remove(p));
                                inner.Insert(_readyPlaylistCounts[ii], ppm[ii][k]);
                                break;
                        }
                        _readyPlaylistCounts[ii]++;
                    }
                });
            }
            if (isPersonalProfile && cacheService is not null)
            {
                await cacheService.SavePersonalPlaylistsAsync(toCache);
            }
            if (playlistsResponse is { HasMore: false }) break;
        }
        for (int i = 0; i <= 2; i++)
        {
            if (_playlistSources[i].Items.Count <= _readyPlaylistCounts[i]) continue;
            var ii = i;
            _playlistSources[i].Edit(inner =>
            {
                inner.RemoveRange(_readyPlaylistCounts[ii], inner.Count - _readyPlaylistCounts[ii]);
            });
        }
        (Application.Current as App)?.CurrentAccount?.QuickAccessPlaylists?.Update(toCache);
    }

    private void AddPlaylist(OnlinePlaylist playlist)
    {
        if (playlist.CreatorId == Parameter &&
            playlist is { Type: PlaylistType.Likelist or PlaylistType.Annual })
        {
            _playlistSources[0].Add(playlist);
        }
        else if (playlist.CreatorId == Parameter ||
                 playlist.Type == PlaylistType.Shared &&
                 playlist.SharedCreators.Any(c => c.UserId == Parameter))
        {
            _playlistSources[1].Add(playlist);
        }
        else
        {
            _playlistSources[2].Add(playlist);
        }
    }

    [RelayCommand]
    private void OnSizeChanged(SizeChangedEventArgs args)
    {
        var width = args.NewSize.Width - 8d * 2;
        var cardPerRow = Math.Ceiling(width / 240d);
        NavigatiableCardWidth = width / cardPerRow;
    }

    public override void AllocateDisposable()
    {
        if (Application.Current is App { CurrentAccount.UserAccount: { } account } && Parameter == account.UserId)
        {
            WeakReferenceMessenger.Default.Register<UserDetailViewModel, OnlinePlaylist, string>(this,
                MessengerTokens.PlaylistDeleted, OnPlaylistRemoved);
            WeakReferenceMessenger.Default.Register<UserDetailViewModel, OnlinePlaylist, string>(this,
                MessengerTokens.PlaylistUnsubscribed, OnPlaylistRemoved);
        }

        return;

        static void OnPlaylistRemoved(UserDetailViewModel viewModel, OnlinePlaylist playlist)
        {
            foreach (var source in viewModel._playlistSources)
                if (source.Remove(playlist)) return;
        }
    }

    public override void RecycleDisposable()
    {
        foreach (var subscription in _playlistSubscriptions)
            subscription.Dispose();
        foreach (var source in _playlistSources)
            source.Dispose();
        WeakReferenceMessenger.Default.Unregister<OnlinePlaylist, string>(this, MessengerTokens.PlaylistDeleted);
        WeakReferenceMessenger.Default.Unregister<OnlinePlaylist, string>(this, MessengerTokens.PlaylistUnsubscribed);
    }
}