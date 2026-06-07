using System.Collections.ObjectModel;
using Cirrus.LiveModels;
using Cirrus.Models.Business.Album;
using Cirrus.Models.Business.Playlist;
using Cirrus.Models.Network.Artist;
using Cirrus.Models.Network.Public;
using Cirrus.Network;
using Cirrus.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Cirrus.ViewModels;

public partial class BrowseViewModel : ViewModel
{
    public override string ViewIdentifier => "Browse";

    [ObservableProperty] public partial Banner[] Banners { get; set; } = [];
    public ObservableCollection<OnlinePlaylist> TrendingPlaylists { get; } = new();
    public ObservableCollection<IndexedOnlineTrack> NewTracks { get; } = new();
    public ObservableCollection<OnlineAlbum> NewAlbums { get; } = new();
    public ObservableCollection<OnlinePlaylist> Charts { get; } = new();
    public ObservableCollection<ArtistDetail> TopArtists { get; } = new();

    public override async Task LoadDataAsync()
    {
        var bannersResponse = await Client.Public.GetBannersAsync();
        Banners = [.. bannersResponse.Banners];

        TrendingPlaylists.Clear();
        var trendingPlaylistResponse = await Client.Public.GetTopPlaylistsAsync();
        foreach (var playlist in trendingPlaylistResponse.Playlists.Select(static p => p.ToBusinessModel()))
        {
            TrendingPlaylists.Add(playlist);
        }

        NewTracks.Clear();
        var newTracksResponse = await Client.Public.GetNewTracksAsync();
        foreach (var track in newTracksResponse.Tracks.Select(static t => t.ToBusinessModel()))
        {
            NewTracks.Add(new() { Track = track });
        }

        NewAlbums.Clear();
        var newAlbumsResponse = await Client.Public.GetNewAlbumsAsync();
        foreach (var album in newAlbumsResponse.Albums.Select(static a => a.ToBusinessModel()))
        {
            NewAlbums.Add(album);
        }

        Charts.Clear();
        var chartsResponse = await Client.Public.GetChartsAsync();
        foreach (var playlist in chartsResponse.Charts.Select(static p => p.ToBusinessModel()))
        {
            Charts.Add(playlist);
        }

        TopArtists.Clear();
        var topArtistsResponse = await Client.Public.GetTopArtistsAsync(20);
        foreach (var artist in topArtistsResponse.Artists)
        {
            TopArtists.Add(artist);
        }
    }
}