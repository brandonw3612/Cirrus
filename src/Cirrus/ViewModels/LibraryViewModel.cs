using System.Collections.ObjectModel;
using Cirrus.Models.Network.Album;
using Cirrus.Models.Network.Artist;
using Cirrus.Network;
using Cirrus.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using WinRT;

namespace Cirrus.ViewModels;

[GeneratedBindableCustomProperty([
    nameof(DisplayCardWidth)
], [])]
public partial class LibraryViewModel : ViewModel
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayCardWidth))]
    [NotifyPropertyChangedFor(nameof(CanLoadMore))]
    public partial int SelectedCategory { get; set; } = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayCardWidth))]
    public partial double ViewWidth { get; set; }

    public double DisplayCardWidth
    {
        get
        {
            var width = Math.Floor(ViewWidth - 16d);
            return Math.Floor(SelectedCategory switch
            {
                0 => width / Math.Ceiling(width / 240d),
                1 => width / Math.Ceiling(width / 160d),
                _ => 0
            });
        }
    }

    public bool CanLoadMore => SelectedCategory switch
    {
        0 => _moreAlbums,
        1 => _moreArtists,
        _ => false
    };

    public ObservableCollection<object> DisplayItems { get; } = new();

    private readonly List<ArtistDetail> _artists = new();
    private readonly List<AlbumDetail2> _albums = new();
    private bool _isLoading;

    private bool _moreAlbums = true, _moreArtists = true;
    
    public override string ViewIdentifier => "Library";
    public override async Task LoadDataAsync()
    {
        _artists.Clear();
        _albums.Clear();
        DisplayItems.Clear();
        _moreAlbums = true;
        _moreArtists = true;

        if (SelectedCategory == 0)
        {
            await SwitchToAlbums();
        }
        else
        {
            SelectedCategory = 0;
        }
    }

    private async Task LoadArtistsAsync()
    {
        if (_isLoading || !_moreArtists) return;
        var prevLoading = IsLoading;
        _isLoading = true;
        IsLoading = true;
        try
        {
            var response = await Client.Account.GetSubscribedArtistsAsync(_artists.Count);
            if (response.Artists is not { } artists) return;
            foreach (var artist in artists)
            {
                _artists.Add(artist);
                DisplayItems.Add(artist);
            }
            _moreArtists = response.HasMore;
            OnPropertyChanged(nameof(CanLoadMore));
        }
        finally
        {
            _isLoading = false;
            IsLoading = prevLoading;
        }
    }
    
    private async Task LoadAlbumsAsync()
    {
        if (_isLoading || !_moreAlbums) return;
        var prevLoading = IsLoading;
        _isLoading = true;
        IsLoading = true;
        try
        {
            var response = await Client.Account.GetSubscribedAlbumsAsync(_albums.Count);
            if (response.Albums is not { } albums) return;
            foreach (var album in albums)
            {
                _albums.Add(album);
                DisplayItems.Add(album);
            }
            _moreAlbums = response.HasMore;
            OnPropertyChanged(nameof(CanLoadMore));
        }
        finally
        {
            _isLoading = false;
            IsLoading = prevLoading;
        }
    }

    private async Task SwitchToArtists()
    {
        DisplayItems.Clear();
        foreach (var artist in _artists)
        {
            DisplayItems.Add(artist);
        }
        if (_artists.Count == 0 && _moreArtists) await LoadArtistsAsync();
    }

    private async Task SwitchToAlbums()
    {
        DisplayItems.Clear();
        foreach (var album in _albums)
        {
            DisplayItems.Add(album);
        }
        if (_albums.Count == 0 && _moreAlbums) await LoadAlbumsAsync();
    }

    async partial void OnSelectedCategoryChanged(int value)
    {
        if (value is 0) await SwitchToAlbums();
        else if (value is 1) await SwitchToArtists();
    }

    [RelayCommand]
    private void OnSizeChanged(SizeChangedEventArgs args) => ViewWidth = args.NewSize.Width;

    [RelayCommand]
    private async Task LoadMoreContentAsync()
    {
        switch (SelectedCategory)
        {
            case 0: await LoadAlbumsAsync(); break;
            case 1: await LoadArtistsAsync(); break;
        }
    }
}