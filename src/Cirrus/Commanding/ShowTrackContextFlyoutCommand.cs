using Windows.Foundation;
using Cirrus.Commanding.Primitives;
using Cirrus.Extensions;
using Cirrus.Generated.Attributes;
using Cirrus.Models.Business.Playback;
using Cirrus.Models.Business.Track;
using Cirrus.Utilities;
using Cirrus.Views;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Cirrus.Commanding;

public sealed partial class ShowTrackContextFlyoutCommand : CommandWrapper
{
    private static ShowTrackContextFlyoutCommand? _instance;
    public static ShowTrackContextFlyoutCommand Instance => _instance ??= new();

    private readonly PlayFromListCommand _playFromListCommand = new();

    public IList<IAudioTrack<ulong>>? ContextList
    {
        get => _playFromListCommand.Tracks;
        set => _playFromListCommand.Tracks = value;
    }
    
    public ShowTrackContextFlyoutCommand()
    {
        InnerCommand = new RelayCommandEx<FrameworkElement, object>((sender, args) =>
        {
            if (!GetPosition(args, sender, out var position))
            {
                position = new(0, 0);
            }
            (sender.DataContext switch
            {
                OnlineTrack track => BuildTrackMenuFlyout(track),
                _ => null
            })?.ShowAt(sender, position);
        });
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
    
    private MenuFlyout? BuildTrackMenuFlyout(OnlineTrack track)
    {
        if (MainWindow.Current is not { } window) return null;
        return BuildBaseMenuFlyout().AddItems([
            new MenuFlyoutItem
            {
                Text = track.Title,
                IsEnabled = false
            },
            ContextList is null ? null : new MenuFlyoutItem
            {
                Icon = new FontIcon{Glyph = "\uE768"},
                Text = "MenuFlyoutItems/Play/Text".GetLocalized() ?? "{InvalidResource}",
                Command = _playFromListCommand,
                CommandParameter = track
            },
            ContextList is null ? null : new MenuFlyoutItem
            {
                Icon = new ImageIcon
                {
                    Source = LocalImageAssetMaintainer.Instance.GetImageSource(
                        window.CurrentTheme switch
                        {
                            ElementTheme.Dark => "ms-appx:///Assets/MultiColorIcons/PlayNext-dark.svg",
                            _ => "ms-appx:///Assets/MultiColorIcons/PlayNext-light.svg"
                        })
                },
                Text = "MenuFlyoutItems/PlayNext/Text".GetLocalized() ?? "{InvalidResource}",
                Command = PrependTrackCommand.Instance,
                CommandParameter = track
            },
            ContextList is null ? null : new MenuFlyoutItem
            {
                Icon = new ImageIcon
                {
                    Source = LocalImageAssetMaintainer.Instance.GetImageSource(
                        window.CurrentTheme switch
                        {
                            ElementTheme.Dark => "ms-appx:///Assets/MultiColorIcons/PlayLast-dark.svg",
                            _ => "ms-appx:///Assets/MultiColorIcons/PlayLast-light.svg"
                        })
                },
                Text = "MenuFlyoutItems/PlayLast/Text".GetLocalized() ?? "{InvalidResource}",
                Command = AppendTrackCommand.Instance,
                CommandParameter = track
            },
            new MenuFlyoutSeparator(),
            new MenuFlyoutItem
            {
                Icon = new FontIcon
                {
                    Glyph = "\uE91C",
                    FontFamily = Application.Current.Resources["CirrusIconSetFontFamily"] as FontFamily
                },
                Text = "MenuFlyoutItems/AddToPlaylist/Text".GetLocalized() ?? "{InvalidResource}",
            },
            new MenuFlyoutSeparator(),
            track.Artists.Length switch
            {
                0 => null,
                1 => new MenuFlyoutItem
                {
                    Icon = new FontIcon
                    {
                        Glyph = "\uE91B",
                        FontFamily = Application.Current.Resources["CirrusIconSetFontFamily"] as FontFamily
                    },
                    Text = track.Artists[0].Name,
                    Command = NavigateCommand.Instance,
                    CommandParameter = track.Artists[0]
                },
                _ => new MenuFlyoutSubItem
                {
                    Icon = new FontIcon
                    {
                        Glyph = "\uE91B",
                        FontFamily = Application.Current.Resources["CirrusIconSetFontFamily"] as FontFamily
                    },
                    Text = "MenuFlyoutItems/Artists/Text".GetLocalized() ?? "{InvalidResource}",
                }.AddItems(track.Artists.Select(ar => new MenuFlyoutItem
                {
                    Text = ar.Name,
                    Command = NavigateCommand.Instance,
                    CommandParameter = ar
                }).OfType<MenuFlyoutItemBase?>().ToArray())
            },
            track.Album is { } album
                ? new MenuFlyoutItem
                {
                    Text = album.Title,
                    Icon = new FontIcon { Glyph = "\uE93C" },
                    Command = NavigateCommand.Instance,
                    CommandParameter = album
                }
                : null,
            new MenuFlyoutSeparator(),
            new MenuFlyoutItem
            {
                Text = "MenuFlyoutItems/Share/Text".GetLocalized() ?? "{InvalidResource}",
                Icon = new FontIcon { Glyph = "\uE72D" },
                Command = ShareCommand.Instance,
                CommandParameter = track
            },
            new MenuFlyoutItem
            {
                Text = "MenuFlyoutItems/CopyLink/Text".GetLocalized() ?? "{InvalidResource}",
                Icon = new FontIcon { Glyph = "\uE71B" },
                Command = CopyLinkCommand.Instance,
                CommandParameter = track
            }
        ]);
    }
    
    [DuckMethod("GetPosition", Assemblies = [
        "Microsoft.WinUI"
    ])]
    private static partial bool GetPosition(object args, FrameworkElement sender, out Point pos);
}
