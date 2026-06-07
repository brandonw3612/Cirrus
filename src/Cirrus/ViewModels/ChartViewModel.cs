using System.Collections.ObjectModel;
using Cirrus.Commanding;
using Cirrus.LiveModels;
using Cirrus.Models.Business.Playback;
using Cirrus.Network;
using Cirrus.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Cirrus.ViewModels;

public partial class ChartViewModel : ViewModel<(string Nickname, ulong UserId)>
{
    public override string ViewIdentifier => $"Chart{Parameter.UserId}";

    public string Nickname => Parameter.Nickname;

    [ObservableProperty] public partial int SelectedCategory { get; set; } = 0;

    [ObservableProperty]
    public partial ShowTrackContextFlyoutCommand? WeeklyTracksShowTrackContextFlyoutCommand { get; set; }

    [ObservableProperty]
    public partial ShowTrackContextFlyoutCommand? AllTimeTracksShowTrackContextFlyoutCommand { get; set; }

    [ObservableProperty] public partial PlayFromListCommand? WeeklyTracksPlayFromListCommand { get; set; }
    [ObservableProperty] public partial PlayFromListCommand? AllTimeTracksPlayFromListCommand { get; set; }

    public ObservableCollection<IndexedOnlineTrack> DisplayItems { get; } = [];
    private bool _isLoading;

    private readonly List<IndexedOnlineTrack> _oneWeekTracks = [];
    private readonly List<IndexedOnlineTrack> _allTimeTracks = [];

    public override async Task LoadDataAsync()
    {
        _oneWeekTracks.Clear();
        _allTimeTracks.Clear();
        DisplayItems.Clear();

        if (SelectedCategory == 0) await SwitchCategory(0);
        else SelectedCategory = 0;
    }

    async partial void OnSelectedCategoryChanged(int value) => await SwitchCategory(value);

    private async Task SwitchCategory(int index)
    {
        DisplayItems.Clear();
        switch (index)
        {
            case 0:
                _oneWeekTracks.ForEach(DisplayItems.Add);
                if (_oneWeekTracks.Count == 0) await LoadOneWeekTracks();
                return;
            case 1:
                _allTimeTracks.ForEach(DisplayItems.Add);
                if (_allTimeTracks.Count == 0) await LoadAllTimeTracks();
                return;
        }
    }

    private async Task LoadOneWeekTracks()
    {
        if (_isLoading) return;
        var prevLoading = IsLoading;
        _isLoading = IsLoading = true;
        try
        {
            var response = await Client.User.GetPlaybackRecordAsync(Parameter.UserId, true);
            if (response is not { StatusCode: 200 }) return;
            var index = 1;
            foreach (var item in response.WeeklyTracks)
            {
                if (item.Track is null) continue;
                IndexedOnlineTrack live = new()
                {
                    Index = index.ToString(),
                    Percentage = item.RelativePlays,
                    Track = item.Track.ToBusinessModel(),
                    Plays = item.Plays.ToString()
                };
                _oneWeekTracks.Add(live);
                DisplayItems.Add(live);
                index++;
            }

            var context = _oneWeekTracks.Select(t => t.Track).OfType<IAudioTrack<ulong>>().ToList();
            WeeklyTracksPlayFromListCommand?.Tracks = context;
            WeeklyTracksShowTrackContextFlyoutCommand?.ContextList = context;
        }
        finally
        {
            _isLoading = false;
            IsLoading = prevLoading;
        }
    }

    private async Task LoadAllTimeTracks()
    {
        if (_isLoading) return;
        var prevLoading = IsLoading;
        _isLoading = IsLoading = true;
        try
        {
            var response = await Client.User.GetPlaybackRecordAsync(Parameter.UserId, false);
            if (response is not { StatusCode: 200 }) return;
            var index = 1;
            foreach (var item in response.AllTimeTracks)
            {
                if (item.Track is null) continue;
                IndexedOnlineTrack live = new()
                {
                    Index = index.ToString(),
                    Percentage = item.RelativePlays,
                    Track = item.Track.ToBusinessModel(),
                    Plays = item.Plays.ToString()
                };
                _allTimeTracks.Add(live);
                DisplayItems.Add(live);
                index++;
            }

            var context = _allTimeTracks.Select(t => t.Track).OfType<IAudioTrack<ulong>>().ToList();
            AllTimeTracksPlayFromListCommand?.Tracks = context;
            AllTimeTracksShowTrackContextFlyoutCommand?.ContextList = context;
        }
        finally
        {
            _isLoading = false;
            IsLoading = prevLoading;
        }
    }

    partial void OnWeeklyTracksPlayFromListCommandChanged(PlayFromListCommand? value)
    {
        value?.Tracks = _oneWeekTracks.Select(t => t.Track).OfType<IAudioTrack<ulong>>().ToList();
    }

    partial void OnWeeklyTracksShowTrackContextFlyoutCommandChanged(ShowTrackContextFlyoutCommand? value)
    {
        value?.ContextList = _oneWeekTracks.Select(t => t.Track).OfType<IAudioTrack<ulong>>().ToList();
    }

    partial void OnAllTimeTracksPlayFromListCommandChanged(PlayFromListCommand? value)
    {
        value?.Tracks = _allTimeTracks.Select(t => t.Track).OfType<IAudioTrack<ulong>>().ToList();
    }

    partial void OnAllTimeTracksShowTrackContextFlyoutCommandChanged(ShowTrackContextFlyoutCommand? value)
    {
        value?.ContextList = _allTimeTracks.Select(t => t.Track).OfType<IAudioTrack<ulong>>().ToList();
    }
}