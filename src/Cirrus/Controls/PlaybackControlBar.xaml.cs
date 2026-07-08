using Cirrus.Extensions;
using Cirrus.ViewModels;
using Cirrus.Views;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Cirrus.Controls;

public sealed partial class PlaybackControlBar
{
    public PlaybackControlBarViewModel ViewModel { get; }
    private bool _isFocused;

    public PlaybackControlBar()
    {
        ViewModel = new();
        InitializeComponent();
    }

    [RelayCommand]
    private void OnSizeChanged(SizeChangedEventArgs args)
    {
        VisualStateManager.GoToState(this, args.NewSize.Width switch
        {
            > 600 => "Full",
            > 400 => "Basic",
            _ => "Compact"
        }, false);
    }

    [RelayCommand]
    private void OnLoaded()
    {
        var backgroundBorderVisual = BackgroundBorder.GetVisual();
        var albumArtworkBorderVisual = AlbumArtworkBorder.GetVisual();
        var trackInfoPanelVisual = TrackInfoPanel.GetVisual();

        backgroundBorderVisual.Scale = new(1f);
        backgroundBorderVisual.Offset = new(0f);
        albumArtworkBorderVisual.Scale = new(1f);
        albumArtworkBorderVisual.Offset = new(6f, 6f, 0f);
        trackInfoPanelVisual.Offset = trackInfoPanelVisual.Offset with { X = 60f };
    }

    [RelayCommand]
    private void OnTapped(TappedRoutedEventArgs args)
    {
        if (MainWindow.Current is not { } window) return;
        var current = args.OriginalSource as DependencyObject;
        while (current != null && current != this)
        {
            if (current is Button or GlidySlider) return;
            current = VisualTreeHelper.GetParent(current);
        }
        window.TogglePlaybackDrawer(true);
    }

    protected override void OnPointerEntered(PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerDeviceType is Microsoft.UI.Input.PointerDeviceType.Touch) return;
        if (_isFocused) return;
        var backgroundBorderVisual = BackgroundBorder.GetVisual();
        var albumArtworkBorderVisual = AlbumArtworkBorder.GetVisual();
        var trackInfoPanelVisual = TrackInfoPanel.GetVisual();
        var compositor = backgroundBorderVisual.Compositor;

        backgroundBorderVisual.StartAnimationGroup(compositor.BuildAnimationGroupWith(
            compositor.BuildVector3KeyFrameAnimation(
                finalValue: new(1f, 1f + 20f / (float)RootGrid.ActualHeight, 1f),
                duration: TimeSpan.FromMilliseconds(500),
                target: nameof(backgroundBorderVisual.Scale)
            ),
            compositor.BuildVector3KeyFrameAnimation(
                finalValue: new(0f, -20f, 0f),
                duration: TimeSpan.FromMilliseconds(500),
                target: nameof(backgroundBorderVisual.Offset)
            )
        ));
        albumArtworkBorderVisual.StartAnimationGroup(compositor.BuildAnimationGroupWith(
            compositor.BuildVector3KeyFrameAnimation(
                finalValue: new(68f / 48f, 68f / 48f, 1f),
                duration: TimeSpan.FromMilliseconds(500),
                target: nameof(albumArtworkBorderVisual.Scale)
            ),
            compositor.BuildVector3KeyFrameAnimation(
                finalValue: new(6f, -14f, 0f),
                duration: TimeSpan.FromMilliseconds(500),
                target: nameof(albumArtworkBorderVisual.Offset)
            )
        ));
        trackInfoPanelVisual.StartAnimation("Offset.X", compositor.BuildScalarKeyFrameAnimation(
            finalValue: 80f,
            duration: TimeSpan.FromMilliseconds(500)
        ));
        TimelineSlider.Visibility = Visibility.Visible;

        _isFocused = true;
        base.OnPointerEntered(e);
    }

    protected override void OnPointerExited(PointerRoutedEventArgs e)
    {
        if (!_isFocused) return;
        var backgroundBorderVisual = BackgroundBorder.GetVisual();
        var albumArtworkBorderVisual = AlbumArtworkBorder.GetVisual();
        var trackInfoPanelVisual = TrackInfoPanel.GetVisual();
        var compositor = backgroundBorderVisual.Compositor;

        backgroundBorderVisual.StartAnimationGroup(compositor.BuildAnimationGroupWith(
            compositor.BuildVector3KeyFrameAnimation(
                finalValue: new(1f),
                duration: TimeSpan.FromMilliseconds(500),
                target: nameof(backgroundBorderVisual.Scale),
                delay: TimeSpan.FromMilliseconds(100)
            ),
            compositor.BuildVector3KeyFrameAnimation(
                finalValue: new(0f),
                duration: TimeSpan.FromMilliseconds(500),
                target: nameof(backgroundBorderVisual.Offset),
                delay: TimeSpan.FromMilliseconds(100)
            )
        ));
        albumArtworkBorderVisual.StartAnimationGroup(compositor.BuildAnimationGroupWith(
            compositor.BuildVector3KeyFrameAnimation(
                finalValue: new(1f),
                duration: TimeSpan.FromMilliseconds(500),
                target: nameof(albumArtworkBorderVisual.Scale),
                delay: TimeSpan.FromMilliseconds(100)
            ),
            compositor.BuildVector3KeyFrameAnimation(
                finalValue: new(6f, 6f, 0f),
                duration: TimeSpan.FromMilliseconds(500),
                target: nameof(albumArtworkBorderVisual.Offset),
                delay: TimeSpan.FromMilliseconds(100)
            )
        ));
        trackInfoPanelVisual.StartAnimation("Offset.X", compositor.BuildScalarKeyFrameAnimation(
            finalValue: 60f,
            duration: TimeSpan.FromMilliseconds(500),
            delay: TimeSpan.FromMilliseconds(100)
        ));
        TimelineSlider.Visibility = Visibility.Collapsed;

        _isFocused = false;
        base.OnPointerExited(e);
    }
}