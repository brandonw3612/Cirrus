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

public abstract class UserDetailViewBase : View<UserDetailViewModel, ulong>;

[GeneratedBindableCustomProperty([
    nameof(ViewModel)
], [])]
public sealed partial class UserDetailView
{
    private bool _isHeaderCompact;
    private readonly DualStateBorderComposition _headerBorderComposition;

    public UserDetailView()
    {
        InitializeComponent();
        var compositor = this.GetVisual().Compositor;
        _headerBorderComposition = DualStateBorderComposition.CreateHeaderBar(compositor);
        HeaderBackgroundBorder.SetChildElementVisual(_headerBorderComposition.Visual);
        HeaderAvatarImage.Dimensions.AnimateDimensions(
            0f, 0f, 200f, 200f,
            1f, 100f,
            TimeSpan.FromMilliseconds(1), TimeSpan.Zero
        );
    }

    [RelayCommand]
    private void OnLoaded()
    {
        _headerBorderComposition.ForceApplyTheme(ActualTheme);

        var scrollViewer = PlaylistGridView.FindDescendant<ScrollViewer>();
        if (scrollViewer is null) return;

        var scrollViewerManipulationProps = scrollViewer.GetManipulationPropertySet();
        var scrollingProperties = scrollViewerManipulationProps.GetSpecializedReference<ManipulationPropertySetReferenceNode>();
        var progressNode = ExpressionFunctions.Clamp(scrollingProperties.Translation.Y, -232, 0);

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
            Math.Min(args.NewSize._width, 1000f) - 64f, 76f,
            1f, 8f
        );
        _headerBorderComposition.ExpandedDimensions = new(
            0f, 0f,
            args.NewSize._width - 32f, 232f,
            1f, 8f
        );
        ViewHeader.Width = _isHeaderCompact ? Math.Min(args.NewSize.Width, 1000d) - 64d : args.NewSize.Width - 32d;
    }
    
    private void UpdateHeaders(double offset)
    {
        var collapseHeader = offset >= 232;
        if (collapseHeader == _isHeaderCompact) return;
        VisualStateManager.GoToState(this, collapseHeader ? "Compact" : "Expanded", true);
        var coverImageSize = collapseHeader ? 50f : 200f;
        HeaderAvatarImage.Dimensions.AnimateDimensions(
            0f, 0f, coverImageSize, coverImageSize,
            1f, collapseHeader ? 25f : 100f,
            TimeSpan.FromMilliseconds(400), TimeSpan.Zero
        );
        ViewHeader.Width = collapseHeader ? Math.Min(ActualWidth, 1000d) - 64d : ActualWidth - 32d;
        _headerBorderComposition.ToggleCompact(collapseHeader);
        _isHeaderCompact = collapseHeader;
    }
}
