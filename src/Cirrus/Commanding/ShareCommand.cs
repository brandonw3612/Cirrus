using Windows.ApplicationModel.DataTransfer;
using Cirrus.Commanding.Primitives;
using Cirrus.Models.Abstract;
using Cirrus.Models.Abstract.Primitives;
using Cirrus.Views;
using CommunityToolkit.Mvvm.Input;
using WinUIEx;

namespace Cirrus.Commanding;

public sealed partial class ShareCommand : CommandWrapper
{
    private static ShareCommand? _instance;
    public static ShareCommand Instance => _instance ??= new();

    public ShareCommand()
    {
        InnerCommand = new RelayCommand<ISharable>(
            static sharable =>
            {
                if (sharable is null || MainWindow.Current is not { } window) return;
                var dataTransferManager = DataTransferManagerInterop.GetForWindow(window.GetWindowHandle());
                dataTransferManager.DataRequested += (_, args) =>
                {
                    SetDataPackage(args.Request.Data, sharable);
                };
                DataTransferManagerInterop.ShowShareUIForWindow(window.GetWindowHandle());
            }
        );
    }

    private static void SetDataPackage(DataPackage package, ISharable sharable)
    {
        switch (sharable)
        {
            case IPlaylist playlist:
            {
                package.Properties.Title = playlist.Title;
                package.SetWebLink(new($"https://music.163.com/playlist?id={playlist.PlaylistId}"));
                break;
            }
            case ITrack track:
            {
                package.Properties.Title = track.Title;
                package.SetWebLink(new($"https://music.163.com/song?id={track.TrackId}"));
                break;
            }
            case IAlbum album:
            {
                package.Properties.Title = album.Title;
                package.SetWebLink(new($"https://music.163.com/album?id={album.AlbumId}"));
                break;
            }
            case IArtist artist:
            {
                package.Properties.Title = artist.Name;
                package.SetWebLink(new($"https://music.163.com/artist?id={artist.ArtistId}"));
                break;
            }
        }
    }
}