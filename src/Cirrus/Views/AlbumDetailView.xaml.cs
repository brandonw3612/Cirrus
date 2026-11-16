using Cirrus.Behaviors;
using Cirrus.Commanding;
using Cirrus.Commanding.Primitives;
using Cirrus.Composition;
using Cirrus.Extensions;
using Cirrus.Primitives;
using Cirrus.ViewModels;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Animations.Expressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;

namespace Cirrus.Views;

public abstract class AlbumDetailViewBase : View<AlbumDetailViewModel, ulong>;

public sealed partial class AlbumDetailView
{
    private bool _isHeaderCompact;
    private readonly DualStateBorderComposition _headerBorderComposition;
    
    public AlbumDetailView()
    {
        InitializeComponent();
        var compositor = this.GetVisual().Compositor;
        _headerBorderComposition = DualStateBorderComposition.CreateHeaderCard(compositor);
        HeaderBackgroundBorder.SetChildElementVisual(_headerBorderComposition.Visual);
        HeaderCoverImage.Dimensions.AnimateDimensions(
            0f, 0f, 240f, 240f,
            1f, 8f,
            TimeSpan.FromMilliseconds(1), TimeSpan.Zero
        );
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
        _headerBorderComposition.ForceApplyTheme(ActualTheme);
        
        var scrollViewer = TrackListView.FindDescendant<ScrollViewer>();
        if (scrollViewer is null) return;
        
        var scrollViewerManipulationProps = scrollViewer.GetManipulationPropertySet();
        var scrollingProperties = scrollViewerManipulationProps.GetSpecializedReference<ManipulationPropertySetReferenceNode>();
        var progressNode = ExpressionFunctions.Clamp(scrollingProperties.Translation.Y, -272, 0);

        var headerVisual = ViewHeader.GetVisual();
        headerVisual.StartAnimation("Translation.Y", progressNode);

        
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

    [RelayCommand]
    private void OnSizeChanged(SizeChangedEventArgs args)
    {
        if (args.NewSize.Width <= 0) return;
        _headerBorderComposition.CompactDimensions = new(
            8f, 8f,
            Math.Min(args.NewSize._width, 1000f) - 64f, 76f,
            1f, 8f
        );
        _headerBorderComposition.ExpandedDimensions = new(
            0f, 0f,
            args.NewSize._width - 32f, 272f,
            1f, 8f
        );
        ViewHeader.Width = _isHeaderCompact ? Math.Min(args.NewSize.Width, 1000d) - 64d : args.NewSize.Width - 32d;
    }
    
    private void UpdateHeaders(double offset)
    {
        var collapseHeader = offset >= 272;
        if (collapseHeader == _isHeaderCompact) return;
        VisualStateManager.GoToState(this, collapseHeader ? "Compact" : "Expanded", true);
        var coverImageSize = collapseHeader ? 60f : 240f;
        HeaderCoverImage.Dimensions.AnimateDimensions(
            0f, 0f, coverImageSize, coverImageSize,
            1f, collapseHeader ? 4f : 8f,
            TimeSpan.FromMilliseconds(400), TimeSpan.Zero
        );
        ViewHeader.Width = collapseHeader ? Math.Min(ActualWidth, 1000d) - 64d : ActualWidth - 32d;
        _headerBorderComposition.ToggleCompact(collapseHeader);
        _isHeaderCompact = collapseHeader;
    }
}