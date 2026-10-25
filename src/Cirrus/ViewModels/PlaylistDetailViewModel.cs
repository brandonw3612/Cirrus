using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Cirrus.Base.Services;
using Cirrus.Base.Services.Abstract;
using Cirrus.Constants;
using Cirrus.Extensions;
using Cirrus.LiveModels;
using Cirrus.Models.Business.Playback;
using Cirrus.Models.Business.Playlist;
using Cirrus.Models.Business.Track;
using Cirrus.Models.Network.Artist;
using Cirrus.Models.Network.Response.Artist;
using Cirrus.Models.Network.Track;
using Cirrus.Network;
using Cirrus.Network.Exceptions;
using Cirrus.Playback.PlaybackQueueProviders;
using Cirrus.Playback.Primitives;
using Cirrus.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using DynamicData;
using Microsoft.UI.Xaml;

namespace Cirrus.ViewModels;

public partial class PlaylistDetailViewModel : ViewModel<ulong>
{
    private readonly SourceList<IndexedOnlineTrack> _tracksSource = new();
    private readonly IDisposable _tracksSubscription;
    private CancellationTokenSource? _loadCancellation;
    private int _loadGeneration;

    public override string ViewIdentifier => $"Playlist{Parameter}";

    [ObservableProperty] public partial OnlinePlaylist? Playlist { get; set; }
    [ObservableProperty] public partial string? UpdatedTimeText { get; set; }
    [ObservableProperty] public partial bool IsDescriptionTeachingTipOpen { get; set; }
    [ObservableProperty] public partial string TrackCountDurationString { get; set; } = string.Empty;
    [ObservableProperty] public partial string CreatedDateString { get; set; } = string.Empty;

    [ObservableProperty] public partial bool IsIntelligentListSupported { get; set; }
    [ObservableProperty] public partial bool IsSubscribable { get; set; }
    [ObservableProperty] public partial bool IsSubscribed { get; set; }

    public ReadOnlyObservableCollection<IndexedOnlineTrack> Tracks { get; }
    public List<IAudioTrack<ulong>> PlayableTracks { get; } = new();
    public ObservableCollection<ArtistDetail2> Artists { get; } = new();

    public PlaylistDetailViewModel()
    {
        var synchronizationContext = ServicesProvider.GetService<ISynchronizationContextService>()?.Get()
            ?? SynchronizationContext.Current
            ?? new SynchronizationContext();
        _tracksSubscription = _tracksSource
            .Connect()
            .ObserveOn(synchronizationContext)
            .Bind(out var tracks)
            .Subscribe();
        Tracks = tracks;
    }

    public override async Task LoadDataAsync()
    {
        if (_loadCancellation != null)
        {
            await _loadCancellation.CancelAsync();
            _loadCancellation?.Dispose();
        }
        _loadCancellation = new();
        var cancellationToken = _loadCancellation.Token;
        var generation = ++_loadGeneration;

        _tracksSource.Clear();
        PlayableTracks.Clear();
        Artists.Clear();

        var cacheService = ServicesProvider.GetService<ICacheService>();
        var currentUserId = (Application.Current as App)?.CurrentAccount?.UserProfile?.UserId;
        var cachedPlaylists = cacheService is null
            ? new Dictionary<ulong, OnlinePlaylist>()
            : await cacheService.GetPlaylistsAsync(new List<ulong> { Parameter }, cancellationToken);

        if (cachedPlaylists.TryGetValue(Parameter, out var cachedPlaylist))
        {
            ApplyPlaylist(cachedPlaylist, currentUserId);
            var trackLoadingTask = LoadPlaylistTracksAsync(
                cachedPlaylist,
                currentUserId,
                cacheService,
                generation,
                cancellationToken);

            var onlinePlaylist = await GetOnlinePlaylistAsync(cancellationToken);
            if (onlinePlaylist is null || !IsCurrent(generation, cancellationToken)) return;
            if ((onlinePlaylist.CreatorId == currentUserId || onlinePlaylist.IsSubscribed) &&
                cacheService is not null)
                await cacheService.SavePlaylistsAsync(new List<OnlinePlaylist> { onlinePlaylist }, cancellationToken);

            if (!cachedPlaylist.TrackIds.SequenceEqual(onlinePlaylist.TrackIds))
            {
                await _loadCancellation.CancelAsync();
                try
                {
                    await trackLoadingTask;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                }
                ++_loadGeneration;
                _loadCancellation.Dispose();
                _loadCancellation = new CancellationTokenSource();
                cancellationToken = _loadCancellation.Token;
                generation = _loadGeneration;
                _tracksSource.Clear();
                PlayableTracks.Clear();
                ApplyPlaylist(onlinePlaylist, currentUserId);
                await LoadPlaylistTracksAsync(
                    onlinePlaylist,
                    currentUserId,
                    cacheService,
                    generation,
                    cancellationToken);
            }
            else
            {
                await trackLoadingTask;
                ApplyPlaylist(onlinePlaylist, currentUserId);
            }
        }
        else
        {
            var onlinePlaylist = await GetOnlinePlaylistAsync(cancellationToken);
            if (onlinePlaylist is null) return;
            if ((onlinePlaylist.CreatorId == currentUserId || onlinePlaylist.IsSubscribed) &&
                cacheService is not null)
                await cacheService.SavePlaylistsAsync(new List<OnlinePlaylist> { onlinePlaylist }, cancellationToken);
            ApplyPlaylist(onlinePlaylist, currentUserId);
            await LoadPlaylistTracksAsync(
                onlinePlaylist,
                currentUserId,
                cacheService,
                generation,
                cancellationToken);
        }

        if (!IsCurrent(generation, cancellationToken) || Playlist is null) return;
        UpdatePlaylistSummary();

        var artists = Tracks
            .SelectMany(static t => t.Track.Artists)
            .DistinctBy(static ar => ar.ArtistId)
            .TakeAtMost(15)
            .ToArray();
        var batchResponse = await Client.Advanced.SendMultipleRequestsAsync(
            artists.Select(static ar => ((string, string, object?))("artist", "details", ar.ArtistId))
                .ToArray()
        );
        if (!IsCurrent(generation, cancellationToken)) return;
        foreach (var artist in batchResponse.OfType<ArtistDetailsApiResponse>().Select(static r => r.Artist))
        {
            if (artist is not null) Artists.Add(artist);
        }
    }

    private async Task<OnlinePlaylist?> GetOnlinePlaylistAsync(CancellationToken cancellationToken)
    {
        var detailResponse = await Client.Playlist.GetDetailsAsync(Parameter);
        cancellationToken.ThrowIfCancellationRequested();
        if (detailResponse.Playlist is not { } playlist) return null;
        return playlist.ToBusinessModel();
    }

    private void ApplyPlaylist(OnlinePlaylist playlist, ulong? currentUserId)
    {
        if (currentUserId == playlist.CreatorId && playlist.Type == PlaylistType.Likelist)
            playlist.Title = "Views/PlaylistDetailView/MyFavoritesTitle".GetLocalized() ?? "{Invalid Resource}";
        Playlist = playlist;
        UpdatedTimeText = Playlist is { Type: PlaylistType.Annual or PlaylistType.Mix }
            ? null
            : string.Format(
                "Views/PlaylistDetailView/NormalPlaylistUpdatedTime/Format".GetLocalized() ?? "{InvalidResource}",
                Playlist.UpdatedTime.ToLocalTime().ToString("d"));
        IsIntelligentListSupported = Playlist.CreatorId == currentUserId && Playlist.Type == PlaylistType.Likelist;
        IsSubscribable = currentUserId is not null && Playlist.CreatorId != currentUserId && !Playlist.IsSubscribed;
        IsSubscribed = currentUserId is not null && Playlist.IsSubscribed;
    }

    private async Task LoadPlaylistTracksAsync(
        OnlinePlaylist playlist,
        ulong? currentUserId,
        ICacheService? cacheService,
        int generation,
        CancellationToken cancellationToken)
    {
        var loadedTracks = cacheService is null
            ? new Dictionary<ulong, OnlineTrack>()
            : await cacheService.GetOnlineTracksAsync(playlist.TrackIds, cancellationToken);
        if (!IsCurrent(generation, cancellationToken)) return;

        UpdateTracks(playlist.TrackIds, loadedTracks);
        var cacheMiss = playlist.TrackIds.Where(id => !loadedTracks.ContainsKey(id)).ToList();
        List<OnlineTrack> tracksToCache = [];
        const int singleGroupCount = 200;
        foreach (var groupIds in cacheMiss.Chunk(singleGroupCount))
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<TrackDetail> fetchedTracks;
            try
            {
                var tracksResponse = await Client.Track.GetDetailsAsync(groupIds.ToList());
                fetchedTracks = tracksResponse.Tracks ?? [];
            }
            catch (UnsuccessfulResponseException ex) when (ex.StatusCode is 404)
            {
                fetchedTracks = [];
            }

            foreach (var track in fetchedTracks)
            {
                var local = track.ToBusinessModel();
                loadedTracks[local.TrackId] = local;
                tracksToCache.Add(local);
            }
            if (!IsCurrent(generation, cancellationToken)) return;
            UpdateTracks(playlist.TrackIds, loadedTracks);
        }

        if (tracksToCache.Count > 0 &&
            (playlist.CreatorId == currentUserId || playlist.IsSubscribed) &&
            cacheService is not null &&
            IsCurrent(generation, cancellationToken))
            await cacheService.SaveOnlineTracksAsync(tracksToCache, cancellationToken);
    }

    private void UpdateTracks(IReadOnlyList<ulong> trackIds, IReadOnlyDictionary<ulong, OnlineTrack> tracks)
    {
        var visibleTracks = trackIds
            .Select((id, index) => tracks.TryGetValue(id, out var track)
                ? new IndexedOnlineTrack { Track = track, Index = (index + 1).ToString() }
                : null)
            .OfType<IndexedOnlineTrack>()
            .ToList();
        _tracksSource.Edit(list =>
        {
            list.Clear();
            list.AddRange(visibleTracks);
        });
        PlayableTracks.Clear();
        PlayableTracks.AddRange(visibleTracks.Select(static t => (IAudioTrack<ulong>)t.Track));
    }

    private void UpdatePlaylistSummary()
    {
        var totalDuration = TimeSpan.FromSeconds(Tracks.Sum(static t => t.Track.Duration.TotalSeconds));
        var durationString = totalDuration.TotalHours < 1
            ? $"{totalDuration.Minutes:0}:{totalDuration.Seconds:00}"
            : $"{Math.Floor(totalDuration.TotalHours):0}:{totalDuration.Minutes:00}:{totalDuration.Seconds:00}";
        TrackCountDurationString =
            string.Format("Views/PlaylistDetailView/TrackCountDurationFormat".GetLocalized() ?? "{InvalidResource}",
                Playlist!.TrackCount, durationString);
        CreatedDateString =
            string.Format("Views/PlaylistDetailView/CreatedDateFormat".GetLocalized() ?? "{InvalidResource}",
                Playlist!.CreatedTime.ToLocalTime().ToString("d"));

    }

    private bool IsCurrent(int generation, CancellationToken cancellationToken) =>
        generation == _loadGeneration && !cancellationToken.IsCancellationRequested;

    [RelayCommand]
    private void ShowDescriptionTeachingTip() => IsDescriptionTeachingTipOpen = true;

    [RelayCommand]
    private async Task StartStationAsync()
    {
        var startingTrackId = PlayableTracks[Random.Shared.Next(0, PlayableTracks.Count)].TrackId;
        var intelligentListResponse =
            await Client.Playlist.GetIntelligentListAsync(startingTrackId, Playlist!.PlaylistId);
        if (ServicesProvider.GetService<IPlaybackService<ulong>>() is not { } playbackService) return;
        playbackService.QueueProvider?.Dispose();
        playbackService.QueueProvider = new PrefetchedIntelligentQueueProvider<ulong>(
            intelligentListResponse.Tracks
                .Select(static t => t.ToBusinessModel())
                .OfType<IAudioTrack<ulong>>()
                .ToArray());
    }

    public override void AllocateDisposable()
    {
        WeakReferenceMessenger.Default.Register(this, MessengerTokens.PlaylistSubscribed,
            static (PlaylistDetailViewModel viewModel, OnlinePlaylist _) =>
            {
                viewModel.IsSubscribable = false;
                viewModel.IsSubscribed = true;
            });
        WeakReferenceMessenger.Default.Register(this, MessengerTokens.PlaylistUnsubscribed,
            static (PlaylistDetailViewModel viewModel, OnlinePlaylist _) =>
            {
                viewModel.IsSubscribable = true;
                viewModel.IsSubscribed = false;
            });
    }

    public override void RecycleDisposable()
    {
        _loadCancellation?.Cancel();
        _loadCancellation?.Dispose();
        _tracksSubscription.Dispose();
        _tracksSource.Dispose();
        WeakReferenceMessenger.Default.Unregister<OnlinePlaylist, string>(this, MessengerTokens.PlaylistSubscribed);
        WeakReferenceMessenger.Default.Unregister<OnlinePlaylist, string>(this, MessengerTokens.PlaylistUnsubscribed);
    }
}