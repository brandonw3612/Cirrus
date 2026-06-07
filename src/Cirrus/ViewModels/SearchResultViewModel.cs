using System.Collections.ObjectModel;
using Cirrus.Commanding;
using Cirrus.LiveModels;
using Cirrus.Models.Business.Playback;
using Cirrus.Models.Business.Track;
using Cirrus.Models.Network.Album;
using Cirrus.Models.Network.Artist;
using Cirrus.Models.Network.Playlist;
using Cirrus.Models.Network.Response.Search;
using Cirrus.Models.Network.Response.Search.GeneralSearchModules;
using Cirrus.Models.Network.User;
using Cirrus.Models.Shared.Search;
using Cirrus.Network;
using Cirrus.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using WinRT;

namespace Cirrus.ViewModels;

[GeneratedBindableCustomProperty([
    nameof(DisplayCardWidth),
    nameof(CircleDisplayCardWidth),
    nameof(SquareDisplayCardWidth),
    nameof(TrackItemWidth),
    nameof(SwitchToCategoryCommand)
], [])]
public partial class SearchResultViewModel : ViewModel<string>
{
    [ObservableProperty] public partial int SelectedCategory { get; set; } = 0;

    public ObservableCollection<object> DisplayItems { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayCardWidth))]
    [NotifyPropertyChangedFor(nameof(CircleDisplayCardWidth))]
    [NotifyPropertyChangedFor(nameof(SquareDisplayCardWidth))]
    [NotifyPropertyChangedFor(nameof(TrackItemWidth))]
    public partial double ViewWidth { get; set; }

    public double DisplayCardWidth
    {
        get
        {
            var width = Math.Floor(ViewWidth - 32d);
            return SelectedCategory switch
            {
                1 => width / Math.Ceiling(width / 160d),
                2 => width / Math.Ceiling(width / 240d),
                4 => width / Math.Ceiling(width / 240d),
                5 => width / Math.Ceiling(width / 160d),
                _ => 0
            };
        }
    }

    public double CircleDisplayCardWidth
    {
        get
        {
            var width = Math.Floor(ViewWidth - 16d);
            return width / Math.Ceiling(width / 160d);
        }
    }

    public double SquareDisplayCardWidth
    {
        get
        {
            var width = Math.Floor(ViewWidth - 16d);
            return width / Math.Ceiling(width / 240d);
        }
    }

    public double TrackItemWidth
    {
        get
        {
            var width = Math.Floor(ViewWidth - 44d);
            return width / Math.Floor(width / 400d);
        }
    }

    [ObservableProperty] public partial bool CanLoadMore { get; set; }

    [ObservableProperty]
    public partial ShowTrackContextFlyoutCommand? TopTracksShowTrackContextFlyoutCommand { get; set; }

    [ObservableProperty]
    public partial ShowTrackContextFlyoutCommand? FullTracksShowTrackContextFlyoutCommand { get; set; }

    [ObservableProperty] public partial PlayFromListCommand? TopTracksPlayFromListCommand { get; set; }
    [ObservableProperty] public partial PlayFromListCommand? FullTracksPlayFromListCommand { get; set; }

    private bool _isLoading;

    private bool _moreArtists = true,
        _moreAlbums = true,
        _moreTracks = true,
        _morePlaylists = true,
        _moreProfiles = true;

    private readonly List<SearchModuleBase> _topResultsModules = [];
    private readonly List<ArtistDetail> _artistResults = [];
    private readonly List<AlbumDetail> _albumResults = [];
    private readonly List<OnlineTrack> _trackResults = [];
    private readonly List<PlaylistDetail> _playlistResults = [];
    private readonly List<UserProfile> _userResults = [];

    public override string ViewIdentifier => $"SearchResult{Parameter}";

    public override async Task LoadDataAsync()
    {
        DisplayItems.Clear();
        _topResultsModules.Clear();
        _artistResults.Clear();
        _albumResults.Clear();
        _trackResults.Clear();
        _playlistResults.Clear();
        _userResults.Clear();
        _moreArtists = true;
        _moreAlbums = true;
        _moreTracks = true;
        _morePlaylists = true;
        _moreProfiles = true;

        if (SelectedCategory == 0)
        {
            await SwitchToCategory(0);
        }
        else
        {
            SelectedCategory = 0;
        }
    }

    private async Task LoadTopResults()
    {
        if (_isLoading) return;
        var prevLoading = IsLoading;
        _isLoading = true;
        IsLoading = true;
        try
        {
            var response = await Client.Search.SearchAsync(Parameter ?? string.Empty, SearchTarget.General);
            if (response is not GeneralSearchApiResponse topResults) return;
            foreach (var module in topResults.Modules)
            {
                if (module is TrackSearchResultModule trackModule)
                {
                    var tracks = trackModule.Tracks.Select(static t => t.ToBusinessModel()).ToList();
                    TopTracksModule localModel = new()
                    {
                        Tracks = tracks
                    };
                    _topResultsModules.Add(localModel);
                    DisplayItems.Add(localModel);
                    var trackContext = tracks.OfType<IAudioTrack<ulong>>().ToList();
                    TopTracksPlayFromListCommand?.Tracks = trackContext;
                    TopTracksShowTrackContextFlyoutCommand?.ContextList = trackContext;
                }
                else
                {
                    _topResultsModules.Add(module);
                    DisplayItems.Add(module);
                }
            }

            CanLoadMore = false;
        }
        finally
        {
            _isLoading = false;
            IsLoading = prevLoading;
        }
    }

    private async Task LoadArtistResults()
    {
        if (_isLoading || !_moreArtists) return;
        var prevLoading = IsLoading;
        _isLoading = true;
        IsLoading = true;
        try
        {
            var response =
                await Client.Search.SearchAsync(Parameter ?? string.Empty, SearchTarget.Artist, _artistResults.Count);
            if (response is not ArtistSearchApiResponse artistResults) return;
            foreach (var artist in artistResults.Artists)
            {
                _artistResults.Add(artist);
                DisplayItems.Add(artist);
            }

            CanLoadMore = _moreArtists = _artistResults.Count < artistResults.TotalCount;
        }
        finally
        {
            _isLoading = false;
            IsLoading = prevLoading;
        }
    }

    private async Task LoadAlbumResults()
    {
        if (_isLoading || !_moreAlbums) return;
        var prevLoading = IsLoading;
        _isLoading = true;
        IsLoading = true;
        try
        {
            var response =
                await Client.Search.SearchAsync(Parameter ?? string.Empty, SearchTarget.Album, _albumResults.Count);
            if (response is not AlbumSearchApiResponse albumResults) return;
            foreach (var album in albumResults.Albums)
            {
                _albumResults.Add(album);
                DisplayItems.Add(album);
            }

            CanLoadMore = _moreAlbums = _albumResults.Count < albumResults.TotalCount;
        }
        finally
        {
            _isLoading = false;
            IsLoading = prevLoading;
        }
    }

    private async Task LoadTrackResults()
    {
        if (_isLoading || !_moreTracks) return;
        var prevLoading = IsLoading;
        _isLoading = true;
        IsLoading = true;
        try
        {
            var response =
                await Client.Search.SearchAsync(Parameter ?? string.Empty, SearchTarget.Track, _trackResults.Count);
            if (response is not TrackSearchApiResponse trackResults) return;
            foreach (var track in trackResults.Tracks)
            {
                var localTrack = track.ToBusinessModel();
                _trackResults.Add(localTrack);
                DisplayItems.Add(localTrack);
            }

            CanLoadMore = _moreTracks = _trackResults.Count < trackResults.TotalCount;
            var context = _trackResults.OfType<IAudioTrack<ulong>>().ToList();
            FullTracksPlayFromListCommand?.Tracks = context;
            FullTracksShowTrackContextFlyoutCommand?.ContextList = context;
        }
        finally
        {
            _isLoading = false;
            IsLoading = prevLoading;
        }
    }

    private async Task LoadPlaylistResults()
    {
        if (_isLoading || !_morePlaylists) return;
        var prevLoading = IsLoading;
        _isLoading = true;
        IsLoading = true;
        try
        {
            var response =
                await Client.Search.SearchAsync(Parameter ?? string.Empty, SearchTarget.Playlist, _trackResults.Count);
            if (response is not PlaylistSearchApiResponse playlistResults) return;
            foreach (var playlist in playlistResults.Playlists)
            {
                _playlistResults.Add(playlist);
                DisplayItems.Add(playlist);
            }

            CanLoadMore = _morePlaylists = _playlistResults.Count < playlistResults.TotalCount;
        }
        finally
        {
            _isLoading = false;
            IsLoading = prevLoading;
        }
    }

    private async Task LoadUserResults()
    {
        if (_isLoading || !_morePlaylists) return;
        var prevLoading = IsLoading;
        _isLoading = true;
        IsLoading = true;
        try
        {
            var response =
                await Client.Search.SearchAsync(Parameter ?? string.Empty, SearchTarget.User, _userResults.Count);
            if (response is not UserSearchApiResponse userResults) return;
            foreach (var user in userResults.Users)
            {
                _userResults.Add(user);
                DisplayItems.Add(user);
            }

            CanLoadMore = _moreProfiles = _userResults.Count < userResults.TotalCount;
        }
        finally
        {
            _isLoading = false;
            IsLoading = prevLoading;
        }
    }

    public RelayCommand<string> SwitchToCategoryCommand => field ??= new(s => SelectedCategory = s switch
    {
        "Artist" => 1,
        "Album" => 2,
        "Track" => 3,
        "Playlist" => 4,
        "User" => 5,
        _ => 0
    });

    private async Task SwitchToCategory(int c)
    {
        DisplayItems.Clear();
        CanLoadMore = false;
        switch (c)
        {
            case 0:
                _topResultsModules.ForEach(DisplayItems.Add);
                if (_topResultsModules.Count == 0) await LoadTopResults();
                CanLoadMore = false;
                return;
            case 1:
                _artistResults.ForEach(DisplayItems.Add);
                if (_artistResults.Count == 0 && _moreArtists) await LoadArtistResults();
                CanLoadMore = _moreArtists;
                return;
            case 2:
                _albumResults.ForEach(DisplayItems.Add);
                if (_albumResults.Count == 0 && _moreAlbums) await LoadAlbumResults();
                CanLoadMore = _moreAlbums;
                return;
            case 3:
                _trackResults.ForEach(DisplayItems.Add);
                if (_trackResults.Count == 0 && _moreTracks) await LoadTrackResults();
                CanLoadMore = _moreTracks;
                return;
            case 4:
                _playlistResults.ForEach(DisplayItems.Add);
                if (_playlistResults.Count == 0 && _morePlaylists) await LoadPlaylistResults();
                CanLoadMore = _morePlaylists;
                return;
            case 5:
                _userResults.ForEach(DisplayItems.Add);
                if (_userResults.Count == 0 && _moreProfiles) await LoadUserResults();
                CanLoadMore = _moreProfiles;
                return;
        }
    }

    async partial void OnSelectedCategoryChanged(int value)
    {
        await SwitchToCategory(value);
    }

    partial void OnTopTracksPlayFromListCommandChanged(PlayFromListCommand? value)
    {
        if (value is null) return;
        if (_topResultsModules.OfType<TopTracksModule>().SingleOrDefault() is not
            { Tracks: { Count: > 0 } tracks }) return;
        value.Tracks = tracks.OfType<IAudioTrack<ulong>>().ToList();
    }

    partial void OnTopTracksShowTrackContextFlyoutCommandChanged(ShowTrackContextFlyoutCommand? value)
    {
        if (value is null) return;
        if (_topResultsModules.OfType<TopTracksModule>().SingleOrDefault() is not
            { Tracks: { Count: > 0 } tracks }) return;
        value.ContextList = tracks.OfType<IAudioTrack<ulong>>().ToList();
    }

    partial void OnFullTracksPlayFromListCommandChanged(PlayFromListCommand? value)
    {
        value?.Tracks = _trackResults.OfType<IAudioTrack<ulong>>().ToList();
    }

    partial void OnFullTracksShowTrackContextFlyoutCommandChanged(ShowTrackContextFlyoutCommand? value)
    {
        value?.ContextList = _trackResults.OfType<IAudioTrack<ulong>>().ToList();
    }

    [RelayCommand]
    private void OnSizeChanged(SizeChangedEventArgs args) => ViewWidth = args.NewSize.Width;

    [RelayCommand]
    private async Task LoadMore()
    {
        switch (SelectedCategory)
        {
            case 1:
                await LoadArtistResults();
                return;
            case 2:
                await LoadAlbumResults();
                return;
            case 3:
                await LoadTrackResults();
                return;
            case 4:
                await LoadPlaylistResults();
                return;
            case 5:
                await LoadUserResults();
                return;
        }
    }
}