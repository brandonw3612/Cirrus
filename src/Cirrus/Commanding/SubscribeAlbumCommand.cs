using Windows.ApplicationModel;
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
    public bool Subscribe { get; set; } = true;
    
    public SubscribeAlbumCommand()
    {
        InnerCommand = new AsyncRelayCommand<OnlineAlbum>(async album =>
        {
            if (album is null || Application.Current is not App app) return;
            var accountId = app.CurrentAccount?.UserAccount?.UserId ?? 0;
            if (accountId == 0 || album.IsSubscribed == Subscribe) return;
            if (MainWindow.Current is not { } window) return;
            var albumId = album.AlbumId;
            if (Subscribe)
            {
                if (await Client.Album.SubscribeAsync(albumId))
                {
                    window.ShowNotification(
                        "Notifications/AlbumSubscriptionSucceeded".GetLocalized() ?? "{InvalidResource}",
                        3000,
                        InfoBarSeverity.Success
                    );
                    album.IsSubscribed = true;
                    WeakReferenceMessenger.Default.Send(album, MessengerTokens.AlbumSubscriptionStatusChanged);
                    return;
                }
                window.ShowNotification(
                    "Notifications/AlbumSubscriptionFailed".GetLocalized() ?? "{InvalidResource}",
                    3000,
                    InfoBarSeverity.Error
                );
            }
            else
            {
                if (await window.DialogController.ShowMessageBoxAsync(
                        Package.Current.DisplayName,
                        "MessageBoxes/UnsubscribeAlbum/Content".GetLocalized() ?? "{InvalidResource}",
                        "MessageBoxes/UnsubscribeAlbum/PrimaryButton/Content".GetLocalized() ?? "{InvalidResource}",
                        "MessageBoxes/UnsubscribeAlbum/CancelButton/Content".GetLocalized() ?? "{InvalidResource}"
                    ) is not ContentDialogResult.Primary) return;
                if (await Client.Album.UnsubscribeAsync(albumId))
                {
                    window.ShowNotification(
                        "Notifications/AlbumUnsubscriptionSucceeded".GetLocalized() ?? "{InvalidResource}",
                        3000,
                        InfoBarSeverity.Success
                    );
                    album.IsSubscribed = false;
                    WeakReferenceMessenger.Default.Send(album, MessengerTokens.AlbumSubscriptionStatusChanged);
                    return;
                }
                window.ShowNotification(
                    "Notifications/AlbumUnsubscriptionFailed".GetLocalized() ?? "{InvalidResource}",
                    3000,
                    InfoBarSeverity.Error
                );
            }
        });
    }
}