using System.Windows.Input;
using Cirrus.Behaviors;
using Cirrus.Models.Business.Track;
using Cirrus.Models.Network.Track;
using Cirrus.ViewModels;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinRT;

namespace Cirrus.Controls;

[GeneratedBindableCustomProperty([
    nameof(ViewModel),
    nameof(AreArtistsShown),
    nameof(DataContext),
    nameof(ShowContextFlyoutCommand)
], [])]
public partial class TrackListItem : Control
{
    public TrackListItemViewModel ViewModel { get; } = new();
    
    public string Index
    {
        get => (string)GetValue(IndexProperty);
        set => SetValue(IndexProperty, value);
    }
    public static readonly DependencyProperty IndexProperty = DependencyProperty.Register(
        nameof(Index), typeof(string), typeof(TrackListItem), new PropertyMetadata(string.Empty));

    public bool AreArtistsShown
    {
        get => (bool)GetValue(AreArtistsShownProperty);
        set => SetValue(AreArtistsShownProperty, value);
    }
    public static readonly DependencyProperty AreArtistsShownProperty = DependencyProperty.Register(
        nameof(AreArtistsShown), typeof(bool), typeof(TrackListItem), new PropertyMetadata(false));
    
    public double PlaysPercentage
    {
        get => (double)GetValue(PlaysPercentageProperty);
        set => SetValue(PlaysPercentageProperty, value);
    }
    public static readonly DependencyProperty PlaysPercentageProperty = DependencyProperty.Register(
        nameof(PlaysPercentage), typeof(double), typeof(TrackListItem), new PropertyMetadata(0d));

    public string Plays
    {
        get => (string)GetValue(PlaysProperty);
        set => SetValue(PlaysProperty, value);
    }
    public static readonly DependencyProperty PlaysProperty = DependencyProperty.Register(
        nameof(Plays), typeof(string), typeof(TrackListItem), new PropertyMetadata(string.Empty));

    public ICommand PlayFromListCommand
    {
        get => (ICommand)GetValue(PlayFromListCommandProperty);
        set => SetValue(PlayFromListCommandProperty, value);
    }
    public static readonly DependencyProperty PlayFromListCommandProperty = DependencyProperty.Register(
        nameof(PlayFromListCommand), typeof(ICommand), typeof(TrackListItem), new PropertyMetadata(null));
    
    public ICommand ShowContextFlyoutCommand
    {
        get => (ICommand)GetValue(ShowContextFlyoutCommandProperty);
        set => SetValue(ShowContextFlyoutCommandProperty, value);
    }
    public static readonly DependencyProperty ShowContextFlyoutCommandProperty = DependencyProperty.Register(
        nameof(ShowContextFlyoutCommand), typeof(ICommand), typeof(TrackListItem), new PropertyMetadata(null));

    [RelayCommand]
    private void OnTrackChanged(DataContextChangedEventArgs args)
    {
        var track = args.NewValue switch
        {
            OnlineTrack t => t,
            TrackDetail t => t.ToBusinessModel(),
            TrackDetail2 t => t.ToBusinessModel(),
            _ => null
        };
        if (track is null) return;
        ViewModel.Title = track.Title;
        ViewModel.IsExplicit = track.IsExplicit;
        ViewModel.Subtitle = string.Join("; ", ((string[])[.. track.TitleTranslations, .. track.Alias]).Distinct());
        ViewModel.AlbumArtworkUrl = track.Album?.ArtworkImageUrl;
        ViewModel.Artists = track.Artists;
        ViewModel.AlbumTitle = track.DisplayAlbum;
        ViewModel.Album = track.Album;
        ViewModel.Duration = $"{Math.Floor(track.Duration.TotalMinutes):0}:{track.Duration.Seconds:00}";
    }

    public TrackListItem()
    {
        DefaultStyleKey = typeof(TrackListItem);
        this.AttachEventTriggerBehaviorToCommand<FrameworkElement, DataContextChangedEventTriggerBehavior>
            (TrackChangedCommand);
    }
}