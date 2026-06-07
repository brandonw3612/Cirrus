using Cirrus.Commanding;
using Cirrus.Commanding.Primitives;
using Cirrus.Primitives;
using Cirrus.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Cirrus.Views;

public abstract class ArtistDetailViewBase : View<ArtistDetailViewModel, ulong>;

public sealed partial class ArtistDetailView
{
    private bool _isFloatingHeaderVisible;
    
    public ArtistDetailView()
    {
        InitializeComponent();
    }

    protected override Task LoadVisualContentAsync()
    {
        (Resources["ShowTrackContextFlyoutCommand"] as ShowTrackContextFlyoutCommand)!.ContextList = ViewModel.PlayableTracks;
        (Resources["PlayFromListCommand"] as PlayFromListCommand)!.Tracks = ViewModel.PlayableTracks;
        UpdateHeaders(0);
        return Task.CompletedTask;
    }

    public RelayCommandEx<ScrollViewer, ScrollViewerViewChangedEventArgs> ScrollViewerViewChangedCommand => field ??=
        new((viewer, _) =>
        {
            UpdateHeaders(viewer.VerticalOffset);
        });
    
    private void UpdateHeaders(double offset)
    {
        var showFloatingHeader = offset > 200 - 50;
        if (showFloatingHeader == _isFloatingHeaderVisible) return;
        if (showFloatingHeader)
        {
            ViewHeader.Opacity = 0;
            ViewHeader.IsHitTestVisible = false;
            FloatingHeader.Visibility = Visibility.Visible;
        }
        else
        {
            ViewHeader.Opacity = 1;
            ViewHeader.IsHitTestVisible = true;
            FloatingHeader.Visibility = Visibility.Collapsed;
        }
        _isFloatingHeaderVisible = showFloatingHeader;
    }
}
