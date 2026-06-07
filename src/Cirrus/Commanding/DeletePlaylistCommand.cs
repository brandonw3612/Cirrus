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

public sealed partial class DeletePlaylistCommand : CommandWrapper
{
    private static DeletePlaylistCommand? _instance;
    public static DeletePlaylistCommand Instance => _instance ??= new();
    
    public DeletePlaylistCommand()
    {
        InnerCommand = new AsyncRelayCommand<OnlinePlaylist>(static async playlist =>
        {
            if (playlist is null || Application.Current is not App app) return;
            var accountId = app.CurrentAccount?.UserAccount?.UserId ?? 0;
            var creatorId = playlist.Creator?.UserId ?? 0;
            if (accountId is 0 || creatorId is 0 || accountId != creatorId ||
                playlist is { Type: PlaylistType.Likelist }) return;
            if (MainWindow.Current is not { } window) return;
            if (await window.DialogController.ShowMessageBoxAsync(
                    Package.Current.DisplayName,
                    "MessageBoxes/DeletePlaylist/Content".GetLocalized() ?? "{InvalidResource}",
                    "MessageBoxes/DeletePlaylist/PrimaryButton/Content".GetLocalized() ?? "{InvalidResource",
                    "Controls/Buttons/Cancel/Content".GetLocalized() ?? "{InvalidResource}"
                ) is not ContentDialogResult.Primary) return;
            var playlistId = playlist.PlaylistId;
            if (await Client.Playlist.RemoveAsync(playlistId))
            {
                window.ShowNotification(
                    "Notifications/PlaylistRemovalSucceeded".GetLocalized() ?? "{InvalidResource}",
                    3000,
                    InfoBarSeverity.Success
                );
                WeakReferenceMessenger.Default.Send(playlist, MessengerTokens.PlaylistDeleted);
                return;
            }
            window.ShowNotification(
                "Notifications/PlaylistRemovalFailed".GetLocalized() ?? "{InvalidResource}",
                3000,
                InfoBarSeverity.Error
            );
        });
    }
}