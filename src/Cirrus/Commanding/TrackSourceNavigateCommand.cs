using Cirrus.Commanding.Primitives;
using Cirrus.Extensions;
using Cirrus.Models.Business.Track;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Cirrus.Commanding;

public sealed partial class TrackSourceNavigateCommand : CommandWrapper
{
    private static TrackSourceNavigateCommand? _instance;
    public static TrackSourceNavigateCommand Instance => _instance ??= new();
    
    public TrackSourceNavigateCommand()
    {
        InnerCommand = new RelayCommandEx<FrameworkElement, TappedRoutedEventArgs>(static (sender, args) =>
        {
            if (sender.DataContext is not OnlineTrack track) return;
            new MenuFlyout
            {
                MenuFlyoutPresenterStyle = new(typeof(MenuFlyoutPresenter))
                {
                    Setters =
                    {
                        new Setter(FrameworkElement.MinWidthProperty, 250d)
                    }
                }
            }.AddItems([
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
            ]).ShowAt(sender, args.GetPosition(sender));
        });
    }
}