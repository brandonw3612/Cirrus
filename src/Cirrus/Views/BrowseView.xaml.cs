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

namespace Cirrus.Views;

public abstract class BrowseViewBase : View<BrowseViewModel>;

public sealed partial class BrowseView
{
    private bool _isHeaderCompact;
    private readonly DualStateBorderComposition _headerBorderComposition;

    public BrowseView()
    {
        InitializeComponent();
        var compositor = this.GetVisual().Compositor;
        _headerBorderComposition = DualStateBorderComposition.CreateHeaderBar(compositor);
        HeaderBackgroundBorder.SetChildElementVisual(_headerBorderComposition.Visual);
    }

    [RelayCommand]
    private void OnLoaded()
    {
        _headerBorderComposition.ForceApplyTheme(ActualTheme);
        
        var manipulationProps = RootScrollViewer.GetManipulationPropertySet();
        var scrollingProperties = manipulationProps.GetSpecializedReference<ManipulationPropertySetReferenceNode>();
        var progressNode = ExpressionFunctions.Clamp(scrollingProperties.Translation.Y, -76, 0);
        var headerVisual = ViewHeader.GetVisual();
        headerVisual.StartAnimation("Translation.Y", progressNode);
        
        Interaction.SetBehaviors(RootScrollViewer, [
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
            Math.Min(args.NewSize._width, 1000f) - 64f, 76f,
            1f, 8f
        );
        _headerBorderComposition.ExpandedDimensions = new(
            0f, 0f,
            args.NewSize._width - 32f, 76f,
            1f, 8f
        );
        ViewHeader.Width = _isHeaderCompact ? Math.Min(args.NewSize.Width, 1000d) - 64d :
                                              args.NewSize.Width - 32d;
    }
    
    private void UpdateHeaders(double offset)
    {
        var collapseHeader = offset >= 76;
        if (collapseHeader == _isHeaderCompact) return;
        VisualStateManager.GoToState(this, collapseHeader ? "Compact" : "Expanded", true);
        ViewHeader.Width = collapseHeader ? Math.Min(ActualWidth, 1000d) - 64d :
                                            ActualWidth - 32d;
        _headerBorderComposition.ToggleCompact(collapseHeader);
        _isHeaderCompact = collapseHeader;
    }
}
