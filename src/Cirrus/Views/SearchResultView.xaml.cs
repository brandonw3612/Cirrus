using Cirrus.Behaviors;
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
using WinRT;

namespace Cirrus.Views;

public abstract class SearchResultViewBase : View<SearchResultViewModel, string>;

[GeneratedBindableCustomProperty([
    nameof(ViewModel)
], [])]
public sealed partial class SearchResultView
{
    private bool _isHeaderCompact;
    private readonly DualStateBorderComposition _headerBorderComposition;

    public SearchResultView()
    {
        InitializeComponent();
        var compositor = this.GetVisual().Compositor;
        _headerBorderComposition = DualStateBorderComposition.CreateHeaderBar(compositor);
        HeaderBackgroundBorder.SetChildElementVisual(_headerBorderComposition.Visual);
    }

    [RelayCommand]
    private void OnLoaded()
    {
        ViewModel.TopTracksShowTrackContextFlyoutCommand = TopTracksShowContextFlyoutCommand;
        ViewModel.FullTracksShowTrackContextFlyoutCommand = FullTracksShowContextFlyoutCommand;
        ViewModel.TopTracksPlayFromListCommand = TopTracksPlayFromListCommand;
        ViewModel.FullTracksPlayFromListCommand = FullTracksPlayFromListCommand;

        _headerBorderComposition.ForceApplyTheme(ActualTheme);
        
        var scrollViewer = ResultListView.FindDescendant<ScrollViewer>();
        if (scrollViewer is null) return;

        var scrollViewerManipulationProps = scrollViewer.GetManipulationPropertySet();
        var scrollingProperties = scrollViewerManipulationProps.GetSpecializedReference<ManipulationPropertySetReferenceNode>();
        var progressNode = ExpressionFunctions.Clamp(scrollingProperties.Translation.Y, -120, 0);

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
            -8f, 0f,
            Math.Min(args.NewSize._width, 1000f) - 64f, 120f,
            1f, 8f
        );
        _headerBorderComposition.ExpandedDimensions = new(
            0f, 0f,
            args.NewSize._width - 32f, 120f,
            1f, 8f
        );
        ViewHeader.Width = _isHeaderCompact ? Math.Min(args.NewSize.Width, 1000d) - 64d : args.NewSize.Width - 32d;
    }
    
    private void UpdateHeaders(double offset)
    {
        var collapseHeader = offset >= 120;
        if (collapseHeader == _isHeaderCompact) return;
        VisualStateManager.GoToState(this, collapseHeader ? "Compact" : "Expanded", true);
        ViewHeader.Width = collapseHeader ? Math.Min(ActualWidth, 1000d) - 64d : ActualWidth - 32d;
        _headerBorderComposition.ToggleCompact(collapseHeader);
        _isHeaderCompact = collapseHeader;
    }
}