using Cirrus.Primitives;
using Cirrus.ViewModels;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Animations.Expressions;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;

namespace Cirrus.Views;

public abstract class HistoryViewBase : View<HistoryViewModel>;

public sealed partial class HistoryView
{
    private CompositionPropertySet? _propertySet;

    public HistoryView()
    {
        InitializeComponent();
    }

    [RelayCommand]
    private void OnLoaded()
    {
        var compositor = HeaderFloatingBackBorder.GetVisual().Compositor;

        var backBorderVisual = HeaderFloatingBackBorder.GetVisual();
        var headerTextVisual = HeaderTextBlock.GetVisual();
        var contentCategorySegmentedVisual = ContentCategorySegmented.GetVisual();

        var scrollViewer = ContentListView.FindDescendant<ScrollViewer>();
        var scrollViewerManipulationPropertySet =
            ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);
        var scrollingProperties = scrollViewerManipulationPropertySet
            .GetSpecializedReference<ManipulationPropertySetReferenceNode>();

        _propertySet = compositor.CreatePropertySet();
        _propertySet.InsertScalar("ViewWidth", (float)ActualWidth);
        _propertySet.InsertScalar("Progress", 0f);

        var viewWidthNode = _propertySet.GetReference().GetScalarProperty("ViewWidth");
        var progressNode = _propertySet.GetReference().GetScalarProperty("Progress");

        var compactWidthNode = ExpressionFunctions.Min(viewWidthNode - 64, 936);
        var textDestination = 0.5f * (viewWidthNode - compactWidthNode) + 32;
        var textOffsetNode = ExpressionFunctions.Lerp(32, textDestination, progressNode);

        _propertySet.StartAnimation("Progress",
            ExpressionFunctions.Clamp(-scrollingProperties.Translation.Y / 100f, 0f, 1f));

        backBorderVisual.StartAnimation("Opacity", progressNode);
        headerTextVisual.StartAnimation("Offset.X", textOffsetNode);
        contentCategorySegmentedVisual.StartAnimation("Offset.X", textOffsetNode * -1);
    }

    [RelayCommand]
    private void OnSizeChanged(SizeChangedEventArgs args)
    {
        if (_propertySet is null) return;
        _propertySet.InsertScalar("ViewWidth", args.NewSize._width);
    }
}
