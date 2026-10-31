using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Cirrus.Base.Services.Abstract;
using Cirrus.Constants;
using Cirrus.Extensions;
using Cirrus.LiveModels;
using Cirrus.Models.Business.Album;
using Cirrus.Models.Business.Playback;
using Cirrus.Models.Business.Track;
using Cirrus.Models.Network.Artist;
using Cirrus.Network;
using Cirrus.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using DynamicData;
using DynamicData.Binding;
using Microsoft.UI.Xaml;

namespace Cirrus.ViewModels;

public partial class AlbumDetailViewModel : ViewModel<ulong>
{
    private readonly SourceList<object> _trackSource = new();
    private readonly IDisposable _trackSubscription;

    [ObservableProperty] public partial OnlineAlbum? Album { get; set; }
    [ObservableProperty] public partial bool IsDescriptionTeachingTipOpen { get; set; }
    [ObservableProperty] public partial string TrackCountDurationString { get; set; } = string.Empty;
    [ObservableProperty] public partial string PublishDateString { get; set; } = string.Empty;
    [ObservableProperty] public partial string CopyrightString { get; set; } = string.Empty;
    [ObservableProperty] public partial bool IsSourceGrouped { get; set; }
    public ReadOnlyObservableCollection<object> TrackItems { get; }
    public List<IAudioTrack<ulong>> PlayableTracks { get; } = new();
    public ObservableCollection<ArtistDetail2> Artists { get; } = new();

    [ObservableProperty] public partial bool IsSubscribable { get; set; }

    [ObservableProperty] public partial bool IsSubscribed { get; set; }

    public override string ViewIdentifier => $"Album{Parameter}";

    public AlbumDetailViewModel()
    {
        var synchronizationContext = ISynchronizationContextService.GetService()?.Get()
            ?? SynchronizationContext.Current
            ?? new SynchronizationContext();
        _trackSubscription = _trackSource
            .Connect()
            .ObserveOn(synchronizationContext)
            .Bind(out var trackItems, BindingOptions.NeverFireReset())
            .Subscribe();
        TrackItems = trackItems;
    }

    public override async Task LoadDataAsync()
    {
        var cacheService = ICacheService.GetService();
        OnlineAlbum nativeAlbum;
        if (cacheService is not null &&
            await cacheService.GetAlbumAsync(Parameter) is { } cachedAlbum)
        {
            nativeAlbum = cachedAlbum;
        }
        else
        {
            var detailResponse = await Client.Album.GetDetailsAsync(Parameter);
            if (detailResponse.Album is not { } album) return;
            nativeAlbum = album.ToBusinessModel();
            nativeAlbum.TrackIds = detailResponse.Tracks.Select(static t => t.TrackId).ToArray();
            if (cacheService is not null)
            {
                await cacheService.SaveAlbumsAsync(new List<OnlineAlbum> { nativeAlbum });
            }
        }
        Album = nativeAlbum;
        var currentUserId = (Application.Current as App)?.CurrentAccount?.UserProfile?.UserId;

        PlayableTracks.Clear();
        var trackIds = nativeAlbum.TrackIds.ToList();
        List<OnlineTrack> tracks;
        Dictionary<ulong, OnlineTrack> cachedTracks = new();
        if (cacheService is not null)
        {
            cachedTracks = await cacheService.GetOnlineTracksAsync(trackIds);
        }
        if (trackIds.Any(id => !cachedTracks.ContainsKey(id))) 
        {
            var tracksResponse = await Client.Track.GetDetailsAsync(trackIds);
            tracks = tracksResponse.Tracks.Select(t => t.ToBusinessModel()).ToList();
            if (cacheService is not null && Album.IsSubscribed)
            {
                await cacheService.SaveOnlineTracksAsync(tracks);
            }
        }
        else
        {
            tracks = trackIds.Select(id => cachedTracks[id]).ToList();
        }
        
        var discsTitles = tracks.Select(static t => t.DiscTitle).Distinct().ToList();
        var albumArtistsIds = Album.Artists.Select(static a => a.ArtistId).ToArray();
        var grouping = discsTitles.Count > 1;
        if (grouping)
        {
            var discs = discsTitles.Select(static t =>
                new AlbumDisc([])
                {
                    DiscTitle = t
                }).ToDictionary(d => d.DiscTitle, d => d);
            foreach (var track in tracks)
            {
                PlayableTracks.Add(track);
                var trackArtistsIds = track.Artists.Select(static a => a.ArtistId).ToArray();
                discs[track.DiscTitle].Add(new()
                {
                    Track = track,
                    AreArtistsShown = !albumArtistsIds.IsEquivalentWith(trackArtistsIds),
                    Index = (discs[track.DiscTitle].Count + 1).ToString()
                });
            }

            IsSourceGrouped = true;
            _trackSource.Edit(items =>
            {
                items.Clear();
                items.AddRange(discs.Values);
            });
        }
        else
        {
            List<IndexedOnlineAlbumTrack> targetTracks = []; 
            foreach (var track in tracks)
            {
                var trackArtistsIds = track.Artists.Select(static a => a.ArtistId).ToArray();
                PlayableTracks.Add(track);
                targetTracks.Add(new()
                {
                    Track = track,
                    AreArtistsShown = !albumArtistsIds.IsEquivalentWith(trackArtistsIds),
                    Index = (targetTracks.Count + 1).ToString()
                });
            }

            IsSourceGrouped = false;
            _trackSource.Edit(items =>
            {
                items.Clear();
                items.AddRange(targetTracks);
            });
        }

        var totalDuration = TimeSpan.FromSeconds(PlayableTracks.Sum(static t => t.Duration.TotalSeconds));
        var durationString = totalDuration.TotalHours < 1
            ? $"{totalDuration.Minutes:0}:{totalDuration.Seconds:00}"
            : $"{Math.Floor(totalDuration.TotalHours):0}:{totalDuration.Minutes:00}:{totalDuration.Seconds:00}";
        TrackCountDurationString =
            string.Format("Views/AlbumDetailView/TrackCountDurationFormat".GetLocalized() ?? "{InvalidResource}",
                nativeAlbum.TrackCount, durationString);
        PublishDateString =
            string.Format("Views/AlbumDetailView/PublishDateFormat".GetLocalized() ?? "{InvalidResource}",
                Album.PublishTime.ToLocalTime().ToString("d"));
        CopyrightString = Album.Company is { Length: > 0 } ? $"(p) {Album.Company}" : string.Empty;

        // var artistsIds = album.Artists.Select(static a => a.ArtistId).ToHashSet();
        // foreach (var artist in PlayableTracks.OfType<OnlineTrack>().SelectMany(static t => t.Artists))
        // {
        //     artistsIds.Add(artist.ArtistId);
        // }
        //
        // var batchResponse = await Client.Advanced.SendMultipleRequestsAsync(
        //     artistsIds.Select(static id => ((string, string, object?))("artist", "details", id))
        //         .ToArray()
        // );
        // foreach (var artist in batchResponse.OfType<ArtistDetailsApiResponse>().Select(static r => r.Artist))
        // {
        //     if (artist is null) continue;
        //     Artists.Add(artist);
        // }
    }
    
    [RelayCommand]
    private void ShowDescriptionTeachingTip() => IsDescriptionTeachingTipOpen = true;

    public override void RecycleDisposable()
    {
        _trackSubscription.Dispose();
        _trackSource.Dispose();
    }
}