using Cirrus.Models.Abstract;
using Cirrus.Models.Shared.Artist;
using CommunityToolkit.Mvvm.ComponentModel;
using WinRT;

namespace Cirrus.ViewModels;

[GeneratedBindableCustomProperty]
public partial class TrackListItemViewModel : ObservableObject
{
    [ObservableProperty] public partial string Title { get; set; } = string.Empty;

    [ObservableProperty] public partial bool IsExplicit { get; set; } = false;

    [ObservableProperty] public partial string Subtitle { get; set; } = string.Empty;

    [ObservableProperty] public partial string? AlbumArtworkUrl { get; set; }

    [ObservableProperty] public partial TrackArtist[] Artists { get; set; } = [];

    [ObservableProperty] public partial string AlbumTitle { get; set; } = string.Empty;

    [ObservableProperty] public partial string Duration { get; set; } = string.Empty;

    [ObservableProperty] public partial IAlbum? Album { get; set; }
}