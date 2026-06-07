using System.Collections.ObjectModel;

namespace Cirrus.LiveModels;

public sealed partial class AlbumDisc(IEnumerable<IndexedOnlineAlbumTrack> tracks)
    : ObservableCollection<IndexedOnlineAlbumTrack>(tracks)
{
    public required string DiscTitle { get; init; }
}