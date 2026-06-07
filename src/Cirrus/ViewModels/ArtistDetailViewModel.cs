using System.Collections.ObjectModel;
using Cirrus.Extensions;
using Cirrus.LiveModels;
using Cirrus.Models.Business.Album;
using Cirrus.Models.Business.Playback;
using Cirrus.Models.Network.Artist;
using Cirrus.Network;
using Cirrus.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Cirrus.ViewModels;

public partial class ArtistDetailViewModel : ViewModel<ulong>
{
    public override string ViewIdentifier => $"Artist{Parameter}";

    [ObservableProperty] public partial ArtistDetail? Artist { get; set; }
    [ObservableProperty] public partial bool IsDescriptionTeachingTipOpen { get; set; }
    [ObservableProperty] public partial string Description { get; set; } = string.Empty;
    public ObservableCollection<IndexedOnlineTrack> Tracks { get; } = new();
    public List<IAudioTrack<ulong>> PlayableTracks { get; } = new();

    public ObservableCollection<OnlineAlbum> FullLengthAlbums { get; } = new();
    public ObservableCollection<OnlineAlbum> SingleAlbums { get; } = new();
    public ObservableCollection<OnlineAlbum> LiveAlbums { get; } = new();

    public ObservableCollection<ArtistDetail> SimilarArtists { get; } = new();

    public override async Task LoadDataAsync()
    {
        var artistDetailResponse = await Client.Artist.GetDetailsAndTracksAsync(Parameter);
        Artist = artistDetailResponse.Artist;
        foreach (var t in artistDetailResponse.Tracks.Select(static track => track.ToBusinessModel()))
        {
            Tracks.Add(new() { Track = t });
            PlayableTracks.Add(t);
        }

        var albumCount = 0;
        while (true)
        {
            var albumResponse = await Client.Artist.GetAlbumsAsync(Parameter, albumCount);
            foreach (var album in albumResponse.Albums.Select(static a => a.ToBusinessModel()))
            {
                (album.AlbumType switch
                {
                    "现场版" => LiveAlbums,
                    "EP" or "Single" or "EP/Single" => SingleAlbums,
                    _ => FullLengthAlbums
                }).Add(album);
            }

            albumCount += albumResponse.Albums.Count;
            if (!albumResponse.HasMore) break;
        }

        Description = artistDetailResponse.Artist?.BriefDescription?.TrimAdvanced() ?? string.Empty;
        var similarArtistsResponse = await Client.Artist.GetSimilarArtistsAsync(Parameter);
        foreach (var artist in similarArtistsResponse.Artists)
        {
            SimilarArtists.Add(artist);
        }
    }

    [RelayCommand]
    private void ShowDescriptionTeachingTip() => IsDescriptionTeachingTipOpen = true;
}