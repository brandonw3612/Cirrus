using Windows.ApplicationModel;
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
    public bool Subscribe { get; set; } = true;

    public static SubscribePlaylistCommand SubscribeInstance => field ??= new() { Subscribe = true };
    
    public static SubscribePlaylistCommand UnsubscribeInstance => field ??= new() { Subscribe = false };
    
    
    public SubscribePlaylistCommand()
    {
        InnerCommand = new AsyncRelayCommand<OnlinePlaylist>(async playlist =>
        {
            if (playlist is null || Application.Current is not App app) return;
            var accountId = app.CurrentAccount?.UserAccount?.UserId ?? 0;
            var creatorId = playlist.Creator?.UserId ?? 0;
            if (accountId == 0 || creatorId == 0 || accountId == creatorId ||
                playlist.IsSubscribed == Subscribe) return;
            if (MainWindow.Current is not { } window) return;
            var playlistId = playlist.PlaylistId;
            if (Subscribe)
            {
                if (await Client.Playlist.SubscribeAsync(playlistId))
                {
                    window.ShowNotification(
                        "Notifications/PlaylistSubscriptionSucceeded".GetLocalized() ?? "{InvalidResource}",
                        3000,
                        InfoBarSeverity.Success
                    );
                    playlist.IsSubscribed = true;
                    WeakReferenceMessenger.Default.Send(playlist, MessengerTokens.PlaylistSubscriptionStatusChanged);
                    return;
                }
                window.ShowNotification(
                    "Notifications/PlaylistSubscriptionFailed".GetLocalized() ?? "{InvalidResource}",
                    3000,
                    InfoBarSeverity.Error
                );
            }
            else
            {
                if (await window.DialogController.ShowMessageBoxAsync(
                        Package.Current.DisplayName,
                        "MessageBoxes/UnsubscribePlaylist/Content".GetLocalized() ?? "{InvalidResource}",
                        "MessageBoxes/UnsubscribePlaylist/PrimaryButton/Content".GetLocalized() ?? "{InvalidResource}",
                        "MessageBoxes/UnsubscribePlaylist/CancelButton/Content".GetLocalized() ?? "{InvalidResource}"
                    ) is not ContentDialogResult.Primary) return;
                if (await Client.Playlist.UnsubscribeAsync(playlistId))
                {
                    window.ShowNotification(
                        "Notifications/PlaylistUnsubscriptionSucceeded".GetLocalized() ?? "{InvalidResource}",
                        3000,
                        InfoBarSeverity.Success
                    );
                    playlist.IsSubscribed = false;
                    WeakReferenceMessenger.Default.Send(playlist, MessengerTokens.PlaylistSubscriptionStatusChanged);
                    return;
                }
                window.ShowNotification(
                    "Notifications/PlaylistUnsubscriptionFailed".GetLocalized() ?? "{InvalidResource}",
                    3000,
                    InfoBarSeverity.Error
                );
            }
        });
    }
}