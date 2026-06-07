using System.Collections.ObjectModel;
using System.Text.Json;
using Windows.Storage;
using Cirrus.Models.Business.Playlist;
using Cirrus.Models.Network.Playlist;
using Cirrus.Network;
using Cirrus.Playback.Extensions;
using Cirrus.Serialization;
using Cirrus.Utilities.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Cirrus.Utilities;

public partial class QuickAccessPlaylistCollection(ulong userId) : ObservableObject
{
    public ObservableCollection<QuickAccessPlaylist> Playlists { get; } = new();

    public QuickAccessPlaylist? FavList { get; private set; }

    public async Task UpdateAsync(IReadOnlyCollection<QuickAccessPlaylist> playlists)
    {
        var playlistsToRemove = Playlists.Except(playlists, QuickAccessPlaylistEqualityComparer.Instance).ToList();
        playlistsToRemove.ForEach(t => Playlists.Remove(t));
        var intersection = playlists
            .Intersect(Playlists, QuickAccessPlaylistEqualityComparer.Instance).ToList();
        Playlists.RearrangeWith(intersection, QuickAccessPlaylistEqualityComparer.Instance);
        int m = 0, n = 0;
        while (m < playlists.Count)
        {
            if (n >= Playlists.Count ||
                !QuickAccessPlaylistEqualityComparer.Instance.Equals(Playlists[n], playlists.ElementAt(m)))
                Playlists.Insert(n, playlists.ElementAt(m));
            m++;
            n++;
        }

        for (var i = 0; i < playlists.Count; i++)
        {
            Playlists[i].Title = playlists.ElementAt(i).Title;
            Playlists[i].CoverImageUrl = playlists.ElementAt(i).CoverImageUrl;
        }

        await SaveAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        List<QuickAccessPlaylist> playlists = new();
        var playlistsGroup1 = await Client.User.GetPlaylistsAsync(userId, 0);
        ProcessPlaylistGroup(playlistsGroup1.Playlists, playlists);
        if (playlistsGroup1.HasMore)
        {
            var playlistsGroup2 = await Client.User.GetPlaylistsAsync(userId, 30);
            ProcessPlaylistGroup(playlistsGroup2.Playlists, playlists);
        }

        await UpdateAsync(playlists);
    }

    private void ProcessPlaylistGroup(List<PlaylistDetail> playlistsInResponse, List<QuickAccessPlaylist> playlists)
    {
        foreach (var playlist in playlistsInResponse)
        {
            if (playlist.Creator?.UserId == userId && playlist.PlaylistType == 5)
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
            foreach (var playlist in playlists)
            {
                Playlists.Add(playlist);
            }
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