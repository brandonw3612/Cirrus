using Cirrus.Models.Business.Playlist;
using Cirrus.Models.Business.User;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Cirrus.Views;

public partial class UserPlaylistDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate? PlaybackRecordTemplate { get; set; }
    public DataTemplate? PlaylistTemplate { get; set; }
    
    protected override DataTemplate SelectTemplateCore(object item) =>
        item switch
        {
            TopChart => PlaybackRecordTemplate,
            OnlinePlaylist => PlaylistTemplate,
            _ => null
        } ?? base.SelectTemplateCore(item);
}