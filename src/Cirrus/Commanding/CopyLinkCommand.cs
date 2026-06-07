using Windows.ApplicationModel.DataTransfer;
using Cirrus.Commanding.Primitives;
using Cirrus.Models.Abstract;
using Cirrus.Models.Abstract.Primitives;
using Cirrus.Views;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;

namespace Cirrus.Commanding;

public sealed partial class CopyLinkCommand : CommandWrapper
{
    private static CopyLinkCommand? _instance;
    public static CopyLinkCommand Instance => _instance ??= new();

    public CopyLinkCommand()
    {
        InnerCommand = new RelayCommand<ISharable>(static sharable =>
            {
                if (sharable is null) return;
                var dataPackage = new DataPackage();
                dataPackage.SetText(GetLink(sharable));
                Clipboard.SetContent(dataPackage);
                if (MainWindow.Current is not { } window) return;
                window.ShowNotification(
                    "Notifications/LinkCopied".GetLocalized() ?? "{InvalidResource}",
                    1000,
                    InfoBarSeverity.Success
                );
            }
        );
    }

    private static string GetLink(ISharable sharable) => sharable switch
    {
        IPlaylist playlist => $"https://music.163.com/playlist?id={playlist.PlaylistId}",
        ITrack track => $"https://music.163.com/song?id={track.TrackId}",
        IAlbum album => $"https://music.163.com/album?id={album.AlbumId}",
        IArtist artist => $"https://music.163.com/artist?id={artist.ArtistId}",
        _ => "https://music.163.com"
    };
}