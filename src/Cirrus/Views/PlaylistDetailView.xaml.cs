using Cirrus.Behaviors;
using Cirrus.Commanding;
using Cirrus.Commanding.Primitives;
using Cirrus.Primitives;
using Cirrus.ViewModels;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;

namespace Cirrus.Views;

public abstract class PlaylistDetailViewBase : View<PlaylistDetailViewModel, ulong>;

public sealed partial class PlaylistDetailView
{
    private bool _isFloatingHeaderVisible;
    
    public PlaylistDetailView()
    {
        InitializeComponent();
    }

    protected override Task LoadVisualContentAsync()
    {
        (Resources["ShowTrackContextFlyoutCommand"] as ShowTrackContextFlyoutCommand)!.ContextList = ViewModel.PlayableTracks;
        (Resources["PlayFromListCommand"] as PlayFromListCommand)!.Tracks = ViewModel.PlayableTracks;
        return base.LoadVisualContentAsync();
    }

    [RelayCommand]
    private void OnLoaded()
    {
        var scrollViewer = TrackListView.FindDescendant<ScrollViewer>();
        if (scrollViewer is null) return;
        
        Interaction.SetBehaviors(scrollViewer, [
            new ViewChangedEventTriggerBehavior
            {
                Actions =
                {
                    new InvokeCommandActionEx
                    {
                        Command = new RelayCommandEx<ScrollViewer, ScrollViewerViewChangedEventArgs>((sender, _) =>
                        {
                            UpdateHeaders(sender.VerticalOffset);
                        }) 
                    }
                }
            }
        ]);
        UpdateHeaders(0);
    }
    
    private void UpdateHeaders(double offset)
    {
        var showFloatingHeader = offset > 240 - 60;
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