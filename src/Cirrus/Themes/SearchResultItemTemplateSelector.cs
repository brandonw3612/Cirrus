using Cirrus.LiveModels;
using Cirrus.Models.Business.Track;
using Cirrus.Models.Network.Album;
using Cirrus.Models.Network.Artist;
using Cirrus.Models.Network.Playlist;
using Cirrus.Models.Network.Response.Search.GeneralSearchModules;
using Cirrus.Models.Network.User;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Cirrus.Themes;

public sealed partial class SearchResultItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? TopArtistModuleTemplate { get; set; }
    public DataTemplate? TopAlbumModuleTemplate { get; set; }
    public DataTemplate? TopTrackModuleTemplate { get; set; }
    public DataTemplate? TopPlaylistModuleTemplate { get; set; }
    public DataTemplate? TopUserModuleTemplate { get; set; }
    public DataTemplate? QuerySuggestionsModuleTemplate { get; set; }
    public DataTemplate? ArtistTemplate { get; set; }
    public DataTemplate? AlbumTemplate { get; set; }
    public DataTemplate? TrackTemplate { get; set; }
    public DataTemplate? PlaylistTemplate { get; set; }
    public DataTemplate? UserTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => item switch
    {
        ArtistSearchResultModule => TopArtistModuleTemplate,
        AlbumSearchResultModule => TopAlbumModuleTemplate,
        TopTracksModule => TopTrackModuleTemplate,
        PlaylistSearchResultModule => TopPlaylistModuleTemplate,
        UserSearchResultModule => TopUserModuleTemplate,
        SimilarQuerySuggestionsModule => QuerySuggestionsModuleTemplate,
        ArtistDetail => ArtistTemplate,
        AlbumDetail => AlbumTemplate,
        OnlineTrack => TrackTemplate,
        PlaylistDetail => PlaylistTemplate,
        UserProfile => UserTemplate,
        _ => null
    } ?? base.SelectTemplateCore(item, container);
}