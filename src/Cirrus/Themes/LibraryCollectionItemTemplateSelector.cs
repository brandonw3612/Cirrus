using Cirrus.Models.Network.Album;
using Cirrus.Models.Network.Artist;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Cirrus.Themes;

public sealed partial class LibraryCollectionItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? AlbumTemplate { get; set; }
    public DataTemplate? ArtistTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) =>
        item switch
        {
            AlbumDetail2 => AlbumTemplate,
            ArtistDetail => ArtistTemplate,
            _ => null
        } ?? base.SelectTemplateCore(item, container);
}