using System.Collections.ObjectModel;
using Cirrus.Extensions;
using Cirrus.LiveModels;
using Cirrus.Models.Business.Album;
using Cirrus.Models.Business.Playback;
using Cirrus.Models.Business.Track;
using Cirrus.Models.Network.Artist;
using Cirrus.Models.Network.Response.Artist;
using Cirrus.Network;
using Cirrus.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Cirrus.ViewModels;

public partial class AlbumDetailViewModel : ViewModel<ulong>
{
    [ObservableProperty] public partial OnlineAlbum? Album { get; set; }
    [ObservableProperty] public partial bool IsDescriptionTeachingTipOpen { get; set; }
    [ObservableProperty] public partial string TrackCountDurationString { get; set; } = string.Empty;
    [ObservableProperty] public partial string PublishDateString { get; set; } = string.Empty;
    [ObservableProperty] public partial string CopyrightString { get; set; } = string.Empty;
    public CollectionViewSource CollectionViewSource { get; } = new();
    public List<IAudioTrack<ulong>> PlayableTracks { get; } = new();
    public ObservableCollection<ArtistDetail2> Artists { get; } = new();

    [ObservableProperty] public partial bool IsSubscribable { get; set; }

    [ObservableProperty] public partial bool IsSubscribed { get; set; }

    public override string ViewIdentifier => $"Album{Parameter}";

    public override async Task LoadDataAsync()
    {
        var detailResponse = await Client.Album.GetDetailsAsync(Parameter);
        if (detailResponse.Album is not { } album) return;
        var currentUserId = (Application.Current as App)?.CurrentAccount?.UserProfile?.UserId;
        Album = album.ToBusinessModel();
        IsSubscribable = currentUserId is not null && !album.IsSubscribed;
        IsSubscribed = currentUserId is not null && album.IsSubscribed;

        PlayableTracks.Clear();
        var tracksResponse =
            await Client.Track.GetDetailsAsync(detailResponse.Tracks.Select(static t => t.TrackId).ToList());
        var discsTitles = tracksResponse.Tracks.Select(static t => t.DiscTitle).Distinct().ToList();
        var albumArtistsIds = album.Artists.Select(static a => a.ArtistId).ToArray();
        if (discsTitles.Count > 1)
        {
            var discs = discsTitles.Select(static t =>
                new AlbumDisc(Array.Empty<IndexedOnlineAlbumTrack>())
                {
                    DiscTitle = t ?? string.Empty
                }).ToObservableCollection();
            foreach (var track in tracksResponse.Tracks)
            {
                var onlineTrack = track.ToBusinessModel();
                PlayableTracks.Add(onlineTrack);
                var trackArtistsIds = onlineTrack.Artists.Select(static a => a.ArtistId).ToArray();
                var trackDisc = discs.First(d => d.DiscTitle == track.DiscTitle);
                trackDisc.Add(new()
                {
                    Track = onlineTrack,
                    AreArtistsShown = !albumArtistsIds.IsEquivalentWith(trackArtistsIds),
                    Index = (trackDisc.Count + 1).ToString()
                });
            }

            CollectionViewSource.IsSourceGrouped = true;
            CollectionViewSource.Source = discs;
        }
        else
        {
            ObservableCollection<IndexedOnlineAlbumTrack> tracks = new();
            foreach (var track in tracksResponse.Tracks)
            {
                var onlineTrack = track.ToBusinessModel();
                var trackArtistsIds = onlineTrack.Artists.Select(static a => a.ArtistId).ToArray();
                PlayableTracks.Add(onlineTrack);
                tracks.Add(new()
                {
                    Track = onlineTrack,
                    AreArtistsShown = !albumArtistsIds.IsEquivalentWith(trackArtistsIds),
                    Index = (tracks.Count + 1).ToString()
                });
            }

            CollectionViewSource.IsSourceGrouped = false;
            CollectionViewSource.Source = tracks;
        }

        var totalDuration = TimeSpan.FromSeconds(PlayableTracks.Sum(static t => t.Duration.TotalSeconds));
        var durationString = totalDuration.TotalHours < 1
            ? $"{totalDuration.Minutes:0}:{totalDuration.Seconds:00}"
            : $"{Math.Floor(totalDuration.TotalHours):0}:{totalDuration.Minutes:00}:{totalDuration.Seconds:00}";
        TrackCountDurationString =
            string.Format("Views/AlbumDetailView/TrackCountDurationFormat".GetLocalized() ?? "{InvalidResource}",
                album.TrackCount, durationString);
        PublishDateString =
            string.Format("Views/AlbumDetailView/PublishDateFormat".GetLocalized() ?? "{InvalidResource}",
                Album.PublishTime.ToLocalTime().ToString("d"));
        CopyrightString = Album.Company is { Length: > 0 } ? $"(p) {Album.Company}" : string.Empty;

        var artistsIds = album.Artists.Select(static a => a.ArtistId).ToHashSet();
        foreach (var artist in PlayableTracks.OfType<OnlineTrack>().SelectMany(static t => t.Artists))
        {
            artistsIds.Add(artist.ArtistId);
        }

        var batchResponse = await Client.Advanced.SendMultipleRequestsAsync(
            artistsIds.Select(static id => ((string, string, object?))("artist", "details", id))
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
}