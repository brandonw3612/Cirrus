using Windows.System;
using Cirrus.Generated.Attributes;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Behaviors;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Cirrus.Behaviors;

public partial class WheelEventRelayBehavior : BehaviorBase<FrameworkElement>
{
    private (ScrollViewer Current, ScrollViewer Parent)? _scrollViewers;

    protected override bool Initialize()
    {
        if (AssociatedObject is not UIElement element) return false;
        var current = element.FindDescendantOrSelf<ScrollViewer>();
        var parent = element.FindAscendant<ScrollViewer>();
        if (current is null || parent is null) return false;
        _scrollViewers = (current, parent);
        current.HorizontalScrollMode = ScrollMode.Auto;
        current.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
        current.VerticalScrollMode = ScrollMode.Disabled;
        current.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
        current.IsScrollInertiaEnabled = true;
        if (VisualTreeHelper.GetChild(current, 0) is UIElement child)
        {
            _ = SetBackground(child, new SolidColorBrush(Colors.Transparent));
            child.PointerWheelChanged += OnCurrentScrollViewerPointerWheelChanged;
        }
        return true;
    }

    protected override bool Uninitialize()
    {
        if (_scrollViewers is not { } scrollViewers) return true;
        if (VisualTreeHelper.GetChild(scrollViewers.Current, 0) is UIElement child)
        {
            child.PointerWheelChanged -= OnCurrentScrollViewerPointerWheelChanged;
        }
        _scrollViewers = null;
        return true;
    }

    private void OnCurrentScrollViewerPointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        if (_scrollViewers is not { } scrollViewers) return;
        var pointProperties = e.GetCurrentPoint(scrollViewers.Current).Properties;
        var wheelDelta = pointProperties.MouseWheelDelta * 3d;
        if (pointProperties.IsHorizontalMouseWheel)
        {
            var current = scrollViewers.Current;
            current.ChangeView(current.HorizontalOffset + wheelDelta, current.VerticalOffset, current.ZoomFactor);
        }
        else if (e.KeyModifiers is VirtualKeyModifiers.Control or VirtualKeyModifiers.Shift)
        {
            var current = scrollViewers.Current;
            current.ChangeView(current.HorizontalOffset - wheelDelta, current.VerticalOffset, current.ZoomFactor);
        }
        else
        {
            var parent = scrollViewers.Parent;
            parent.ChangeView(parent.HorizontalOffset, parent.VerticalOffset - wheelDelta, parent.ZoomFactor);
        }
        e.Handled = true;
    }

    [DuckPropertySetter("Background")]
    private static partial bool SetBackground(UIElement elem, Brush background);
}
