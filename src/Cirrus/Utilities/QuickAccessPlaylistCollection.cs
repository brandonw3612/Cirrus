using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Text.Json;
using Windows.Storage;
using Cirrus.Base.Services;
using Cirrus.Base.Services.Abstract;
using Cirrus.Models.Business.Playlist;
using Cirrus.Models.Network.Playlist;
using Cirrus.Network;
using Cirrus.Playback.Extensions;
using Cirrus.Serialization;
using Cirrus.Utilities.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;

namespace Cirrus.Utilities;

public partial class QuickAccessPlaylistCollection : ObservableObject
{
    private readonly ulong _userId;
    
    private readonly SourceList<QuickAccessPlaylist> _playlistsSource;
    private readonly ReadOnlyObservableCollection<QuickAccessPlaylist> _playlists;
    public ReadOnlyObservableCollection<QuickAccessPlaylist> Playlists => _playlists;

    public QuickAccessPlaylist? FavList { get; private set; }

    public QuickAccessPlaylistCollection(ulong userId)
    {
        _userId = userId;
        
        _playlistsSource = new();
        var synchronizationContext = ServicesProvider.GetService<ISynchronizationContextService>()!.Get();
        _playlistsSource
            .Connect()
            .ObserveOn(synchronizationContext)
            .Bind(out _playlists)
            .Subscribe();
    }
    
    public async Task UpdateAsync(IReadOnlyCollection<QuickAccessPlaylist> playlists)
    {
        _playlistsSource.Edit(inner =>
        {
            var comparer = EqualityComparer<QuickAccessPlaylist>.Default;
            var playlistsToRemove = inner.Except(playlists, comparer).ToList();
            playlistsToRemove.ForEach(t => inner.Remove(t));
            var intersection = playlists.Intersect(inner, comparer).ToList();
            inner.RearrangeWith(intersection, comparer);
            int m = 0, n = 0;
            while (m < playlists.Count)
            {
                if (n >= inner.Count || !comparer.Equals(inner[n], playlists.ElementAt(m)))
                    inner.Insert(n, playlists.ElementAt(m));
                m++;
                n++;
            }
        });

        for (var i = 0; i < playlists.Count; i++)
        {
            _playlistsSource.Items[i].Title = playlists.ElementAt(i).Title;
            _playlistsSource.Items[i].CoverImageUrl = playlists.ElementAt(i).CoverImageUrl;
        }

        await SaveAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        List<QuickAccessPlaylist> playlists = new();
        var playlistsGroup1 = await Client.User.GetPlaylistsAsync(_userId, 0);
        ProcessPlaylistGroup(playlistsGroup1.Playlists, playlists);
        if (playlistsGroup1.HasMore)
        {
            var playlistsGroup2 = await Client.User.GetPlaylistsAsync(_userId, 30);
            ProcessPlaylistGroup(playlistsGroup2.Playlists, playlists);
        }

        await UpdateAsync(playlists);
    }

    private void ProcessPlaylistGroup(List<PlaylistDetail> playlistsInResponse, List<QuickAccessPlaylist> playlists)
    {
        foreach (var playlist in playlistsInResponse)
        {
            if (playlist.Creator?.UserId == _userId && playlist.PlaylistType == 5)
            {
                FavList = playlist.ToBusinessModel().ToQuickAccess();
            }
            else
            {
                if (playlists.Count >= 50) continue;
                playlists.Add(playlist.ToBusinessModel().ToQuickAccess());
            }
        }
    }

    public async Task LoadAsync()
    {
        var localStorage = ApplicationData.Current.LocalFolder.Path;
        var filePath = Path.Join(localStorage, "qap");
        try
        {
            var jsonContent = await File.ReadAllTextAsync(filePath);
            if (JsonSerializer.Deserialize<QuickAccess>(jsonContent, AppSerializationContext.Default.QuickAccess) is not
                { } qa) return;
            if (qa.FavListId is not { } favListId) return;
            FavList = new()
            {
                PlaylistId = favListId,
                Title = string.Empty,
                CoverImageUrl = null
            };
            OnPropertyChanged(nameof(FavList));
            if (qa.Playlists is not { } playlists) return;
            _playlistsSource.Edit(inner =>
            {
                inner.AddRange(playlists);
            });
        }
        catch
        {
            // Ignored.
        }
    }

    private async Task SaveAsync()
    {
        var localStorage = ApplicationData.Current.LocalFolder.Path;
        var filePath = Path.Join(localStorage, "qap");
        if (File.Exists(filePath)) File.Delete(filePath);
        QuickAccess qa = new()
        {
            FavListId = FavList?.PlaylistId,
            Playlists = Playlists.ToArray()
        };
        var json = JsonSerializer.Serialize(qa, AppSerializationContext.Default.QuickAccess);
        await File.WriteAllTextAsync(filePath, json);
    }
}