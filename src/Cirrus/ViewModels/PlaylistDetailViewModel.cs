using System.Collections.ObjectModel;
using Cirrus.Base.Services;
using Cirrus.Constants;
using Cirrus.Extensions;
using Cirrus.LiveModels;
using Cirrus.Models.Business.Playback;
using Cirrus.Models.Business.Playlist;
using Cirrus.Models.Network.Artist;
using Cirrus.Models.Network.Response.Artist;
using Cirrus.Network;
using Cirrus.Playback.PlaybackQueueProviders;
using Cirrus.Playback.Primitives;
using Cirrus.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace Cirrus.ViewModels;

public partial class PlaylistDetailViewModel : ViewModel<ulong>
{
    public override string ViewIdentifier => $"Playlist{Parameter}";

    [ObservableProperty] public partial OnlinePlaylist? Playlist { get; set; }
    [ObservableProperty] public partial string? UpdatedTimeText { get; set; }
    [ObservableProperty] public partial bool IsDescriptionTeachingTipOpen { get; set; }
    [ObservableProperty] public partial string TrackCountDurationString { get; set; } = string.Empty;
    [ObservableProperty] public partial string CreatedDateString { get; set; } = string.Empty;

    [ObservableProperty] public partial bool IsIntelligentListSupported { get; set; }
    [ObservableProperty] public partial bool IsSubscribable { get; set; }
    [ObservableProperty] public partial bool IsSubscribed { get; set; }

    public ObservableCollection<IndexedOnlineTrack> Tracks { get; } = new();
    public List<IAudioTrack<ulong>> PlayableTracks { get; } = new();
    public ObservableCollection<ArtistDetail2> Artists { get; } = new();

    public override async Task LoadDataAsync()
    {
        var detailResponse = await Client.Playlist.GetDetailsAsync(Parameter);
        if (detailResponse.Playlist is not { } playlist) return;
        var currentUserId = (Application.Current as App)?.CurrentAccount?.UserProfile?.UserId;
        if (currentUserId == playlist.CreatorId && playlist.PlaylistType == 5)
            playlist.Title = "Views/PlaylistDetailView/MyFavoritesTitle".GetLocalized() ?? "{Invalid Resource}";
        Playlist = playlist.ToBusinessModel();
        UpdatedTimeText = Playlist is { Type: PlaylistType.Annual or PlaylistType.Mix }
            ? null
            : string.Format(
                "Views/PlaylistDetailView/NormalPlaylistUpdatedTime/Format".GetLocalized() ?? "{InvalidResource}",
                Playlist.UpdatedTime.ToLocalTime().ToString("d"));
        IsIntelligentListSupported = Playlist.CreatorId == currentUserId && Playlist.Type == PlaylistType.Likelist;
        IsSubscribable = currentUserId is not null && Playlist.CreatorId != currentUserId && !Playlist.IsSubscribed;
        IsSubscribed = currentUserId is not null && Playlist.IsSubscribed;
        var tracksAdded = 0;
        var index = 1;
        const int singleGroupCount = 200;
        while (tracksAdded < playlist.TrackIds.Count)
        {
            var groupIds = playlist.TrackIds.Skip(tracksAdded).TakeAtMost(singleGroupCount).ToList();
            var tracksResponse = await Client.Track.GetDetailsAsync(groupIds);
            if (tracksResponse.Tracks is not { } tracks) return;
            foreach (var track in tracks)
            {
                var local = track.ToBusinessModel();
                Tracks.Add(new()
                {
                    Track = local,
                    Index = index++.ToString()
                });
                PlayableTracks.Add(local);
            }

            tracksAdded += groupIds.Count;
        }

        var totalDuration = TimeSpan.FromSeconds(Tracks.Sum(static t => t.Track.Duration.TotalSeconds));
        var durationString = totalDuration.TotalHours < 1
            ? $"{totalDuration.Minutes:0}:{totalDuration.Seconds:00}"
            : $"{Math.Floor(totalDuration.TotalHours):0}:{totalDuration.Minutes:00}:{totalDuration.Seconds:00}";
        TrackCountDurationString =
            string.Format("Views/PlaylistDetailView/TrackCountDurationFormat".GetLocalized() ?? "{InvalidResource}",
                Playlist.TrackCount, durationString);
        CreatedDateString =
            string.Format("Views/PlaylistDetailView/CreatedDateFormat".GetLocalized() ?? "{InvalidResource}",
                Playlist.CreatedTime.ToLocalTime().ToString("d"));

        var artists = Tracks
            .SelectMany(static t => t.Track.Artists)
            .DistinctBy(static ar => ar.ArtistId)
            .TakeAtMost(15)
            .ToArray();
        var batchResponse = await Client.Advanced.SendMultipleRequestsAsync(
            artists.Select(static ar => ((string, string, object?))("artist", "details", ar.ArtistId))
                .ToArray()
        );
        foreach (var artist in batchResponse.OfType<ArtistDetailsApiResponse>().Select(static r => r.Artist))
        {
            if (artist is null) continue;
            Artists.Add(artist);
        }
    }

    [RelayCommand]
    private void ShowDescriptionTeachingTip() => IsDescriptionTeachingTipOpen = true;

    [RelayCommand]
    private async Task StartStationAsync()
    {
        var startingTrackId = PlayableTracks[Random.Shared.Next(0, PlayableTracks.Count)].TrackId;
        var intelligentListResponse =
            await Client.Playlist.GetIntelligentListAsync(startingTrackId, Playlist!.PlaylistId);
        if (ServicesProvider.Current.GetService<IPlaybackService<ulong>>() is not { } playbackService) return;
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
        WeakReferenceMessenger.Default.Unregister<OnlinePlaylist, string>(this, MessengerTokens.PlaylistSubscribed);
        WeakReferenceMessenger.Default.Unregister<OnlinePlaylist, string>(this, MessengerTokens.PlaylistUnsubscribed);
    }
}