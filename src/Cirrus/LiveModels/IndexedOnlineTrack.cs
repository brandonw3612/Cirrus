using Cirrus.Models.Business.Track;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Cirrus.LiveModels;

public partial class IndexedOnlineTrack : ObservableObject
{
    public required OnlineTrack Track { get; init; }
    [ObservableProperty] public partial string Index { get; set; } = string.Empty;
    public double Percentage { get; set; } = 0d;
    public string Plays { get; set; } = string.Empty;
}

public partial class IndexedOnlineAlbumTrack : IndexedOnlineTrack
{
    public required bool AreArtistsShown { get; init; }
}