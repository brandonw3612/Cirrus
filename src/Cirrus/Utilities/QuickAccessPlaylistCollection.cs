using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Cirrus.Base.Extensions;
using Cirrus.Base.Services.Abstract;
using Cirrus.Extensions;
using Cirrus.Models.Business.Playlist;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;

namespace Cirrus.Utilities;

public partial class QuickAccessPlaylistCollection : ObservableObject, IDisposable
{
    private readonly ulong _userId;
    
    private readonly SourceList<QuickAccessPlaylist> _playlistsSource;
    private readonly IDisposable _playlistsSubscription;
    private readonly ReadOnlyObservableCollection<QuickAccessPlaylist> _playlists;
    public ReadOnlyObservableCollection<QuickAccessPlaylist> Playlists => _playlists;

    public QuickAccessPlaylist? FavList { get; private set; }

    public QuickAccessPlaylistCollection(ulong userId)
    {
        _userId = userId;
        
        _playlistsSource = new();
        var synchronizationContext = ISynchronizationContextService.GetService()!.Get();
        _playlistsSubscription = _playlistsSource
            .Connect()
            .ObserveOn(synchronizationContext)
            .Bind(out _playlists)
            .Subscribe();
    }
    
    public void Update(IReadOnlyCollection<OnlinePlaylist> playlists)
    {
        FavList = playlists
            .FirstOrDefault(p => p.Creator?.UserId == _userId && p.Type == PlaylistType.Likelist)
            ?.ToQuickAccess();
        var pls = playlists
            .TakeAtMost(60)
            .Where(p => p.Creator?.UserId != _userId || p.Type != PlaylistType.Likelist)
            .Select(p => p.ToQuickAccess())
            .ToList();
        _playlistsSource.Edit(inner =>
        {
            var comparer = QuickAccessPlaylistEqualityComparer.Default;
            var playlistsToRemove = inner.Except(pls, comparer).ToList();
            playlistsToRemove.ForEach(t => inner.Remove(t));
            var intersection = pls.Intersect(inner, comparer).ToList();
            inner.RearrangeWith(intersection, comparer);
            int m = 0, n = 0;
            while (m < pls.Count)
            {
                if (n >= inner.Count || !comparer.Equals(inner[n], pls[m]))
                    inner.Insert(n, pls[m]);
                m++;
                n++;
            }
        });

        for (var i = 0; i < pls.Count; i++)
        {
            _playlistsSource.Items[i].Title = pls[i].Title;
            _playlistsSource.Items[i].CoverImageUrl = pls[i].CoverImageUrl;
        }
    }

    public async Task LoadAsync()
    {
        if (ICacheService.GetService() is not { } cacheService) return;
        try
        {
            var cachedPlaylists = await cacheService.GetPersonalPlaylistsAsync().TakeAtMostAsync(60).ToListAsync();
            var favList = cachedPlaylists.FirstOrDefault(p => p.Creator?.UserId == _userId && p.Type == PlaylistType.Likelist);
            FavList = favList?.ToQuickAccess();
            OnPropertyChanged(nameof(FavList));
            if (favList is not null) cachedPlaylists.Remove(favList);
            _playlistsSource.Edit(inner =>
            {
                inner.AddRange(cachedPlaylists.Select(p => p.ToQuickAccess()));
            });
        }
        catch
        {
            // Ignored.
        }
    }

    public void Dispose()
    {
        _playlistsSubscription.Dispose();
        _playlistsSource.Dispose();
    }
}