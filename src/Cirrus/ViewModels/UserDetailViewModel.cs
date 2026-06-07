using Cirrus.Constants;
using Cirrus.LiveModels;
using Cirrus.Models.Business.Playlist;
using Cirrus.Models.Business.User;
using Cirrus.Models.Network.User;
using Cirrus.Network;
using Cirrus.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using WinRT;

namespace Cirrus.ViewModels;

[GeneratedBindableCustomProperty([
    nameof(NavigatiableCardWidth)
], [])]
public partial class UserDetailViewModel : ViewModel<ulong>
{
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

    public override async Task LoadDataAsync()
    {
        var detailsResponse = await Client.User.GetDetailsAsync(Parameter);
        Profile = detailsResponse.Profile;
        var playlistCount = 0;
        Playlists[0].Add(new TopChart
        {
            UserId = Parameter,
            Nickname = Profile?.Nickname ?? string.Empty,
            IsAccessible = detailsResponse.IsPlaybackRecordPublic ||
                           Application.Current is App { CurrentAccount.UserAccount: { } currentAccount } &&
                           Parameter == currentAccount.UserId,
            PlayedTrackCount = detailsResponse.TrackCount
        });
        while (true)
        {
            var playlistsResponse = await Client.User.GetPlaylistsAsync(Parameter, playlistCount);
            foreach (var playlist in playlistsResponse.Playlists)
            {
                var onlinePlaylist = playlist.ToBusinessModel();
                if (onlinePlaylist.CreatorId == Parameter &&
                    onlinePlaylist is { Type: PlaylistType.Likelist or PlaylistType.Annual })
                {
                    Playlists[0].Add(onlinePlaylist);
                }
                else if (onlinePlaylist.CreatorId == Parameter ||
                         onlinePlaylist.Type == PlaylistType.Shared &&
                         onlinePlaylist.SharedCreators.Any(c => c.UserId == Parameter))
                {
                    Playlists[1].Add(onlinePlaylist);
                }
                else
                {
                    Playlists[2].Add(onlinePlaylist);
                }

                playlistCount++;
            }

            if (playlistsResponse is { HasMore: false } or { Playlists.Count: <= 0 }) break;
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
            foreach (var group in viewModel.Playlists)
            {
                if (!group.Contains(playlist)) continue;
                group.Remove(playlist);
                return;
            }
        }
    }

    public override void RecycleDisposable()
    {
        WeakReferenceMessenger.Default.Unregister<OnlinePlaylist, string>(this, MessengerTokens.PlaylistDeleted);
        WeakReferenceMessenger.Default.Unregister<OnlinePlaylist, string>(this, MessengerTokens.PlaylistUnsubscribed);
    }
}