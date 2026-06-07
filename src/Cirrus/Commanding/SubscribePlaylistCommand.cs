using Cirrus.Commanding.Primitives;
using Cirrus.Constants;
using Cirrus.Models.Business.Playlist;
using Cirrus.Network;
using Cirrus.Views;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Cirrus.Commanding;

public sealed partial class SubscribePlaylistCommand : CommandWrapper
{
    private static SubscribePlaylistCommand? _instance;
    public static SubscribePlaylistCommand Instance => _instance ??= new();
    
    public SubscribePlaylistCommand()
    {
        InnerCommand = new AsyncRelayCommand<OnlinePlaylist>(static async playlist =>
        {
            if (playlist is null || Application.Current is not App app) return;
            var accountId = app.CurrentAccount?.UserAccount?.UserId ?? 0;
            var creatorId = playlist.Creator?.UserId ?? 0;
            if (accountId == 0 || creatorId == 0 || accountId == creatorId ||
                playlist is { IsSubscribed: true }) return;
            if (MainWindow.Current is not { } window) return;
            var playlistId = playlist.PlaylistId;
            if (await Client.Playlist.SubscribeAsync(playlistId))
            {
                window.ShowNotification(
                    "Notifications/PlaylistSubscriptionSucceeded".GetLocalized() ?? "{InvalidResource}",
                    3000,
                    InfoBarSeverity.Success
                );
                playlist.IsSubscribed = true;
                WeakReferenceMessenger.Default.Send(playlist, MessengerTokens.PlaylistSubscribed);
                return;
            }
            window.ShowNotification(
                "Notifications/PlaylistSubscriptionFailed".GetLocalized() ?? "{InvalidResource}",
                3000,
                InfoBarSeverity.Error
            );
        });
    }
}