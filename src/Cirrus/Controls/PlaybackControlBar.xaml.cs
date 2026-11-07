using Cirrus.Composition;
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

    private readonly ThemeAwareDualStateBorderComposition _borderComposition;

    public PlaybackControlBar()
    {
        ViewModel = new();
        InitializeComponent();
        var compositor = this.GetVisual().Compositor;
        _borderComposition = new(compositor, false);
        _borderComposition.ToggleCompact(true);
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
        _borderComposition.CompactDimensions = new(
            0f, 0f,
            args.NewSize._width, args.NewSize._height,
            1f, 8f
        );
        _borderComposition.ExpandedDimensions = new(
            0f, -20f,
            args.NewSize._width, args.NewSize._height + 20f,
            1f, 8f
        );
    }

    [RelayCommand]
    private void OnLoaded()
    {
        _borderComposition.ForceApplyTheme(ActualTheme);
        BackgroundBorder.SetChildElementVisual(_borderComposition.Visual);
        var albumArtworkBorderVisual = AlbumArtworkBorder.GetVisual();
        var trackInfoPanelVisual = TrackInfoPanel.GetVisual();

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
        var compositor = this.GetVisual().Compositor;
        var albumArtworkBorderVisual = AlbumArtworkBorder.GetVisual();
        var trackInfoPanelVisual = TrackInfoPanel.GetVisual();

        _borderComposition.ToggleCompact(false);
        
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
        var compositor = this.GetVisual().Compositor;
        var albumArtworkBorderVisual = AlbumArtworkBorder.GetVisual();
        var trackInfoPanelVisual = TrackInfoPanel.GetVisual();

        _borderComposition.ToggleCompact(true);
        
        albumArtworkBorderVisual.StartAnimationGroup(compositor.BuildAnimationGroupWith(
            compositor.BuildVector3KeyFrameAnimation(
                finalValue: new(1f),
                duration: TimeSpan.FromMilliseconds(400),
                target: nameof(albumArtworkBorderVisual.Scale)
            ),
            compositor.BuildVector3KeyFrameAnimation(
                finalValue: new(6f, 6f, 0f),
                duration: TimeSpan.FromMilliseconds(400),
                target: nameof(albumArtworkBorderVisual.Offset)
            )
        ));
        trackInfoPanelVisual.StartAnimation("Offset.X", compositor.BuildScalarKeyFrameAnimation(
            finalValue: 60f,
            duration: TimeSpan.FromMilliseconds(400)
        ));
        TimelineSlider.Visibility = Visibility.Collapsed;

        _isFocused = false;
        base.OnPointerExited(e);
    }
}