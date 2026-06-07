using Windows.System;
using Cirrus.Commanding.Primitives;
using Cirrus.Models.Abstract;
using Cirrus.Models.Abstract.Primitives;
using Cirrus.Models.Business.Developer;
using Cirrus.Views;
using CommunityToolkit.Mvvm.Input;

namespace Cirrus.Commanding;

public sealed partial class NavigateCommand : CommandWrapper
{
    private static NavigateCommand? _instance;
    public static NavigateCommand Instance => _instance ??= new();

    public NavigateCommand()
    {
        InnerCommand = new AsyncRelayCommand<INavigatiable>(async static navigatiable =>
            {
                if (MainWindow.Current is not { } window) return;
                switch (navigatiable)
                {
                    case IPlaylist playlist:
                        window.Navigate(typeof(PlaylistDetailView), playlist.PlaylistId);
                        return;
                    case IUser user:
                        window.Navigate(typeof(UserDetailView), user.UserId);
                        return;
                    case IAlbum album:
                        window.Navigate(typeof(AlbumDetailView), album.AlbumId);
                        return;
                    case IArtist artist:
                        window.Navigate(typeof(ArtistDetailView), artist.ArtistId);
                        return;
                    case ITopChart chart:
                        if (chart.IsAccessible) window.Navigate(typeof(ChartView), (chart.Nickname, chart.UserId));
                        return;
                    case IQuery query:
                        window.Navigate(typeof(SearchResultView), query.Keyword);
                        return;
                    case ISoftwareUpdate update:
                        await Launcher.LaunchUriAsync(new(update.DownloadUrl));
                        break;
                }
            }
        );
    }
}