using Cirrus.Commanding.Primitives;
using Cirrus.Constants;
using Cirrus.Models.Business.Album;
using Cirrus.Network;
using Cirrus.Views;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Cirrus.Commanding;

public sealed partial class SubscribeAlbumCommand : CommandWrapper
{
    public SubscribeAlbumCommand()
    {
        InnerCommand = new AsyncRelayCommand<OnlineAlbum>(static async album =>
        {
            if (album is null || Application.Current is not App app) return;
            var accountId = app.CurrentAccount?.UserAccount?.UserId ?? 0;
            if (accountId == 0 || album is { IsSubscribed: true }) return;
            if (MainWindow.Current is not { } window) return;
            var albumId = album.AlbumId;
            if (await Client.Album.SubscribeAsync(albumId))
            {
                window.ShowNotification(
                    "Notifications/PlaylistSubscriptionSucceeded".GetLocalized() ?? "{InvalidResource}",
                    3000,
                    InfoBarSeverity.Success
                );
                album.IsSubscribed = true;
                WeakReferenceMessenger.Default.Send(album, MessengerTokens.AlbumSubscribed);
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