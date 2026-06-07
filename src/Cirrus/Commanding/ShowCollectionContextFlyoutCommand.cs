using Windows.Foundation;
using Cirrus.Commanding.Primitives;
using Cirrus.Extensions;
using Cirrus.Generated.Attributes;
using Cirrus.Models.Business.Album;
using Cirrus.Models.Business.Playlist;
using Cirrus.Models.Business.User;
using Cirrus.Models.Network.Album;
using Cirrus.Models.Network.Playlist;
using CommunityToolkit.WinUI;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Cirrus.Commanding;

public sealed partial class ShowCollectionContextFlyoutCommand : CommandWrapper
{
    private static ShowCollectionContextFlyoutCommand? _instance; 
    public static ShowCollectionContextFlyoutCommand Instance => _instance ??= new();
    
    public ShowCollectionContextFlyoutCommand()
    {
        InnerCommand = new RelayCommandEx<FrameworkElement, object>(static (sender, args) =>
            {
                if (!GetPosition(args, sender, out var position))
                {
                    position = new(0, 0);
                }
                (sender.DataContext switch
                {
                    OnlinePlaylist playlist => BuildPlaylistMenuFlyout(playlist),
                    PlaylistDetail playlist => BuildPlaylistMenuFlyout(playlist.ToBusinessModel()),
                    TopChart playbackRecord => BuildChartMenuFlyout(playbackRecord),
                    OnlineAlbum album => BuildAlbumMenuFlyout(album),
                    AlbumDetail album => BuildAlbumMenuFlyout(album),
                    AlbumDetail2 album => BuildAlbumMenuFlyout(album),
                    _ => null
                })?.ShowAt(sender, position);
            }
        );
    }

    private static MenuFlyout BuildBaseMenuFlyout() => new()
    {
        MenuFlyoutPresenterStyle = new Style(typeof(MenuFlyoutPresenter))
        {
            Setters =
            {
                new Setter(FrameworkElement.MinWidthProperty, 250d)
            }
        }
    };

    private static MenuFlyout BuildChartMenuFlyout(TopChart topChart)
    {
        return BuildBaseMenuFlyout().AddItems([
            new MenuFlyoutItem
            {
                Text = string.Format("Views/UserDetailView/PlaybackRecord/Title/Format".GetLocalized() ?? "{InvalidResource}", topChart.Nickname),
                IsEnabled = false
            },
            new MenuFlyoutItem
            {
                Text = "MenuFlyoutItems/Play/Text".GetLocalized() ?? "{InvalidResource}",
                Icon = new FontIcon { Glyph = "\uE768" },
                Command = PlayCollectionCommand.Instance,
                CommandParameter = topChart,
                IsEnabled = topChart.IsAccessible
            }
        ]);
    }

    private static MenuFlyout? BuildPlaylistMenuFlyout(OnlinePlaylist playlist)
    {
        if (Application.Current is not App app) return null;
        var accountId = app.CurrentAccount?.UserAccount?.UserId ?? 0;
        var creatorId = playlist.Creator?.UserId ?? 0;
        var canSubscribe = accountId > 0 && (creatorId > 0 && accountId != creatorId || playlist.Type is PlaylistType.Chart) &&
                           playlist is { IsSubscribed: false };
        var canDelete = accountId > 0 && creatorId > 0 && accountId == creatorId &&
                        playlist is { Type: not PlaylistType.Likelist };
        var canUnsubscribe = accountId > 0 && (creatorId > 0 && accountId != creatorId || playlist.Type is PlaylistType.Chart) &&
                             playlist is { IsSubscribed: true };
        return BuildBaseMenuFlyout().AddItems([
            new MenuFlyoutItem
            {
                Text = playlist.Title,
                IsEnabled = false
            },
            new MenuFlyoutItem
            {
                Text = "MenuFlyoutItems/Play/Text".GetLocalized() ?? "{InvalidResource}",
                Icon = new FontIcon { Glyph = "\uE768" },
                Command = PlayCollectionCommand.Instance,
                CommandParameter = playlist
            },
            canSubscribe ? new MenuFlyoutItem
            {
                Text = "MenuFlyoutItems/Subscribe/Text".GetLocalized() ?? "{InvalidResource}",
                Icon = new FontIcon { Glyph = "\uED0E" },
                Command = SubscribePlaylistCommand.Instance,
                CommandParameter = playlist
            } : null,
            new MenuFlyoutSeparator(),
            playlist.Creator is not null
                ? new MenuFlyoutItem
                {
                    Text = playlist.Creator.Nickname,
                    Icon = new FontIcon { Glyph = "\uE77B" },
                    Command = NavigateCommand.Instance,
                    CommandParameter = playlist.Creator
                } : null,
            playlist.Creator is not null ? new MenuFlyoutSeparator() : null,
            new MenuFlyoutItem
            {
                Text = "MenuFlyoutItems/Share/Text".GetLocalized() ?? "{InvalidResource}",
                Icon = new FontIcon { Glyph = "\uE72D" },
                Command = ShareCommand.Instance,
                CommandParameter = playlist
            },
            new MenuFlyoutItem
            {
                Text = "MenuFlyoutItems/CopyLink/Text".GetLocalized() ?? "{InvalidResource}",
                Icon = new FontIcon { Glyph = "\uE71B" },
                Command = CopyLinkCommand.Instance,
                CommandParameter = playlist
            },
            canDelete || canUnsubscribe ? new MenuFlyoutSeparator() : null,
            canDelete ? new MenuFlyoutItem
            {
                Text = "MenuFlyoutItems/Delete/Text".GetLocalized() ?? "{InvalidResource}",
                Icon = new FontIcon { Glyph = "\uE74D" },
                Foreground = new SolidColorBrush(Colors.Red),
                Command = DeletePlaylistCommand.Instance,
                CommandParameter = playlist
            } : null,
            canUnsubscribe ? new MenuFlyoutItem
            {
                Text = "MenuFlyoutItems/Unsubscribe/Text".GetLocalized() ?? "{InvalidResource}",
                Icon = new FontIcon { Glyph = "\uE74D" },
                Foreground = new SolidColorBrush(Colors.Red),
                Command = UnsubscribePlaylistCommand.Instance,
                CommandParameter = playlist
            } : null
        ]);
    }

    private static MenuFlyout? BuildAlbumMenuFlyout(OnlineAlbum album)
    {
        if (Application.Current is not App app) return null;
        var accountId = app.CurrentAccount?.UserAccount?.UserId ?? 0;
        var canSubscribe = accountId > 0 && album is { IsSubscribed: false };
        var canUnsubscribe = accountId > 0 && album is { IsSubscribed: true };
        return BuildBaseMenuFlyout().AddItems([
            new MenuFlyoutItem
            {
                Text = album.Title,
                IsEnabled = false
            },
            new MenuFlyoutItem
            {
                Text = "MenuFlyoutItems/Play/Text".GetLocalized() ?? "{InvalidResource}",
                Icon = new FontIcon { Glyph = "\uE768" },
                Command = PlayCollectionCommand.Instance,
                CommandParameter = album
            },
            canSubscribe ? new MenuFlyoutItem
            {
                Text = "MenuFlyoutItems/Subscribe/Text".GetLocalized() ?? "{InvalidResource}",
                Icon = new FontIcon { Glyph = "\uED0E" },
            } : null,
            album.Artists.Length > 0 ? new MenuFlyoutSeparator() : null,
            album.Artists.Length switch
            {
                0 => null,
                1 => new MenuFlyoutItem
                {
                    Icon = new FontIcon
                    {
                        Glyph = "\uE91B",
                        FontFamily = Application.Current.Resources["CirrusIconSetFontFamily"] as FontFamily
                    },
                    Text = album.Artists[0].Name,
                    Command = NavigateCommand.Instance,
                    CommandParameter = album.Artists[0]
                },
                _ => new MenuFlyoutSubItem
                {
                    Icon = new FontIcon
                    {
                        Glyph = "\uE91B",
                        FontFamily = Application.Current.Resources["CirrusIconSetFontFamily"] as FontFamily
                    },
                    Text = "MenuFlyoutItems/Artists/Text".GetLocalized() ?? "{InvalidResource}",
                }.AddItems(album.Artists.Select(static ar => new MenuFlyoutItem
                {
                    Text = ar.Name,
                    Command = NavigateCommand.Instance,
                    CommandParameter = ar
                }).OfType<MenuFlyoutItemBase?>().ToArray())
            },
            new MenuFlyoutSeparator(),
            new MenuFlyoutItem
            {
                Text = "MenuFlyoutItems/Share/Text".GetLocalized() ?? "{InvalidResource}",
                Icon = new FontIcon { Glyph = "\uE72D" },
                Command = ShareCommand.Instance,
                CommandParameter = album
            },
            new MenuFlyoutItem
            {
                Text = "MenuFlyoutItems/CopyLink/Text".GetLocalized() ?? "{InvalidResource}",
                Icon = new FontIcon { Glyph = "\uE71B" },
                Command = CopyLinkCommand.Instance,
                CommandParameter = album
            },
            canUnsubscribe ? new MenuFlyoutSeparator() : null,
            canUnsubscribe ? new MenuFlyoutItem
            {
                Text = "MenuFlyoutItems/Unsubscribe/Text".GetLocalized() ?? "{InvalidResource}",
                Icon = new FontIcon { Glyph = "\uE74D" },
                Foreground = new SolidColorBrush(Colors.Red),
            } : null
        ]);
    }

    private static MenuFlyout? BuildAlbumMenuFlyout(AlbumDetail album)
    {
        if (Application.Current is not App app) return null;
        var accountId = app.CurrentAccount?.UserAccount?.UserId ?? 0;
        var canSubscribe = accountId > 0 && album is { IsSubscribed: false };
        var canUnsubscribe = accountId > 0 && album is { IsSubscribed: true };
        return BuildBaseMenuFlyout().AddItems([
            new MenuFlyoutItem
            {
                Text = album.Title,
                IsEnabled = false
            },
            new MenuFlyoutItem
            {
                Text = "MenuFlyoutItems/Play/Text".GetLocalized() ?? "{InvalidResource}",
                Icon = new FontIcon { Glyph = "\uE768" },
                Command = PlayCollectionCommand.Instance,
                CommandParameter = album
            },
            canSubscribe ? new MenuFlyoutItem
            {
                Text = "MenuFlyoutItems/Subscribe/Text".GetLocalized() ?? "{InvalidResource}",
                Icon = new FontIcon { Glyph = "\uED0E" },
            } : null,
            album.Artists.Count > 0 ? new MenuFlyoutSeparator() : null,
            album.Artists.Count switch
            {
                0 => null,
                1 => new MenuFlyoutItem
                {
                    Icon = new FontIcon
                    {
                        Glyph = "\uE91B",
                        FontFamily = Application.Current.Resources["CirrusIconSetFontFamily"] as FontFamily
                    },
                    Text = album.Artists[0].Name,
                    Command = NavigateCommand.Instance,
                    CommandParameter = album.Artists[0]
                },
                _ => new MenuFlyoutSubItem
                {
                    Icon = new FontIcon
                    {
                        Glyph = "\uE91B",
                        FontFamily = Application.Current.Resources["CirrusIconSetFontFamily"] as FontFamily
                    },
                    Text = "MenuFlyoutItems/Artists/Text".GetLocalized() ?? "{InvalidResource}",
                }.AddItems(album.Artists.Select(static ar => new MenuFlyoutItem
                {
                    Text = ar.Name,
                    Command = NavigateCommand.Instance,
                    CommandParameter = ar
                }).OfType<MenuFlyoutItemBase?>().ToArray())
            },
            new MenuFlyoutSeparator(),
            new MenuFlyoutItem
            {
                Text = "MenuFlyoutItems/Share/Text".GetLocalized() ?? "{InvalidResource}",
                Icon = new FontIcon { Glyph = "\uE72D" },
                Command = ShareCommand.Instance,
                CommandParameter = album
            },
            new MenuFlyoutItem
            {
                Text = "MenuFlyoutItems/CopyLink/Text".GetLocalized() ?? "{InvalidResource}",
                Icon = new FontIcon { Glyph = "\uE71B" },
                Command = CopyLinkCommand.Instance,
                CommandParameter = album
            },
            canUnsubscribe ? new MenuFlyoutSeparator() : null,
            canUnsubscribe ? new MenuFlyoutItem
            {
                Text = "MenuFlyoutItems/Unsubscribe/Text".GetLocalized() ?? "{InvalidResource}",
                Icon = new FontIcon { Glyph = "\uE74D" },
                Foreground = new SolidColorBrush(Colors.Red),
            } : null
        ]);
    }

    private static MenuFlyout? BuildAlbumMenuFlyout(AlbumDetail2 album)
    {
        if (Application.Current is not App app) return null;
        var accountId = app.CurrentAccount?.UserAccount?.UserId ?? 0;
        return BuildBaseMenuFlyout().AddItems([
            new MenuFlyoutItem
            {
                Text = album.Title,
                IsEnabled = false
            },
            new MenuFlyoutItem
            {
                Text = "MenuFlyoutItems/Play/Text".GetLocalized() ?? "{InvalidResource}",
                Icon = new FontIcon { Glyph = "\uE768" },
                Command = PlayCollectionCommand.Instance,
                CommandParameter = album
            },
            album.Artists.Count > 0 ? new MenuFlyoutSeparator() : null,
            album.Artists.Count switch
            {
                0 => null,
                1 => new MenuFlyoutItem
                {
                    Icon = new FontIcon
                    {
                        Glyph = "\uE91B",
                        FontFamily = Application.Current.Resources["CirrusIconSetFontFamily"] as FontFamily
                    },
                    Text = album.Artists[0].Name,
                    Command = NavigateCommand.Instance,
                    CommandParameter = album.Artists[0]
                },
                _ => new MenuFlyoutSubItem
                {
                    Icon = new FontIcon
                    {
                        Glyph = "\uE91B",
                        FontFamily = Application.Current.Resources["CirrusIconSetFontFamily"] as FontFamily
                    },
                    Text = "MenuFlyoutItems/Artists/Text".GetLocalized() ?? "{InvalidResource}",
                }.AddItems(album.Artists.Select(static ar => new MenuFlyoutItem
                {
                    Text = ar.Name,
                    Command = NavigateCommand.Instance,
                    CommandParameter = ar
                }).OfType<MenuFlyoutItemBase?>().ToArray())
            },
            new MenuFlyoutSeparator(),
            new MenuFlyoutItem
            {
                Text = "MenuFlyoutItems/Share/Text".GetLocalized() ?? "{InvalidResource}",
                Icon = new FontIcon { Glyph = "\uE72D" },
                Command = ShareCommand.Instance,
                CommandParameter = album
            },
            new MenuFlyoutItem
            {
                Text = "MenuFlyoutItems/CopyLink/Text".GetLocalized() ?? "{InvalidResource}",
                Icon = new FontIcon { Glyph = "\uE71B" },
                Command = CopyLinkCommand.Instance,
                CommandParameter = album
            }
        ]);
    }

    [DuckMethod("GetPosition", Assemblies = [
        "Microsoft.WinUI"
    ])]
    private static partial bool GetPosition(object args, FrameworkElement sender, out Point pos);
}
