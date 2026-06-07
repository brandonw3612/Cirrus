using Windows.Foundation;
using Cirrus.ViewModels;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Cirrus.Controls;

public sealed partial class CirculatedFlipViewPanel : Panel
{
    private CirculatedFlipViewPanelViewModel ViewModel { get; } = new();
    
    public double ItemSpacing
    {
        get => (double)GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    public static readonly DependencyProperty ItemSpacingProperty =
        DependencyProperty.Register(nameof(ItemSpacing), typeof(double), typeof(CirculatedFlipViewPanel),
            new PropertyMetadata(0, OnLayoutRelatedPropertyChanged));

    public double MinimumEndPadding
    {
        get => (double)GetValue(MinimumEndPaddingProperty);
        set => SetValue(MinimumEndPaddingProperty, value);
    }

    public static readonly DependencyProperty MinimumEndPaddingProperty =
        DependencyProperty.Register(nameof(MinimumEndPadding), typeof(double), typeof(CirculatedFlipViewPanel),
            new PropertyMetadata(0d, OnLayoutRelatedPropertyChanged));

    private (int Index, Rect Rect)[] _currentLayout = Array.Empty<(int, Rect)>();
    
    private static void OnLayoutRelatedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CirculatedFlipViewPanel panel) return;
        panel.InvalidateMeasure();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Children.Count is 0) return new(0, 0);
        var childrenCount = Children.Count;
        var maxWidth = availableSize._width - 2 * MinimumEndPadding;
        // Determine the max height.
        var maxHeight = double.PositiveInfinity;
        Size maxSizePossible = new(maxWidth, availableSize.Height);
        for (var i = 0; i < childrenCount; i++)
        {
            Children[i].Measure(maxSizePossible);
            maxHeight = Math.Min(maxHeight, Children[i].DesiredSize.Height);
        }
        // Re-measure under the constraint of the max height.
        Size constrainedSize = new(maxWidth, maxHeight);
        foreach (var child in Children)
        {
            child.Measure(constrainedSize);
        }
        // Determine whether the elements fill the panel.
        var totalWidth = Children.Sum(child => child.DesiredSize.Width) + ItemSpacing * (childrenCount - 1);
        return totalWidth <= maxWidth ? new Size(totalWidth, maxHeight) : new Size(availableSize.Width, maxHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Children.Count is 0) return new(0, 0);
        var childrenCount = Children.Count;
        var totalWidth = Children.Sum(child => child.DesiredSize.Width) + ItemSpacing * (childrenCount - 1);
        if (totalWidth <= finalSize.Width)
        {
            var offset = 0d;
            for (var i = 0; i < childrenCount; i++)
            {
                Children[i].Arrange(new(new(0, 0), Children[i].DesiredSize));
                Children[i].GetVisual().Offset = new((float)offset, 0, 0);
                offset += Children[i].DesiredSize.Width;
            }
            return finalSize;
        }
        var currentLayout = ComputeLayout(ViewModel.CurrentLeadingIndex, finalSize.Width);
        foreach (var (index, rect) in currentLayout)
        {
            Children[index].Arrange(rect with { X = 0 });
            Children[index].GetVisual().Offset = new((float)rect.Left, 0, 0);
        }
        _currentLayout = currentLayout;
        if (currentLayout[0].Index <= currentLayout[^1].Index)
        {
            for (var i = 0; i < childrenCount; i++)
            {
                if (i >= currentLayout[0].Index && i <= currentLayout[^1].Index) continue;
                Children[i].Arrange(new(new(0, 0), Children[i].DesiredSize));
                Children[i].GetVisual().Offset = new((float)(finalSize.Width + ItemSpacing), 0, 0);
            }
        }
        else
        {
            for (var i = 0; i < childrenCount; i++)
            {
                if (i >= currentLayout[0].Index || i <= currentLayout[^1].Index) continue;
                Children[i].Arrange(new(new(0, 0), Children[i].DesiredSize));
                Children[i].GetVisual().Offset = new((float)(finalSize.Width + ItemSpacing), 0, 0);
            }
        }
        return finalSize;
    }
    
    public void Translate(bool forward)
    {
        if (Children.Count is 0) return;
        var childrenCount = Children.Count;
        var totalWidth = Children.Sum(child => child.DesiredSize.Width) + ItemSpacing * (childrenCount - 1);
        if (totalWidth <= ActualWidth) return;
        var updatedLeadingIndex = forward
            ? (ViewModel.CurrentLeadingIndex + 1) % childrenCount
            : (ViewModel.CurrentLeadingIndex - 1 + childrenCount) % childrenCount;
        var updatedLayout = ComputeLayout(updatedLeadingIndex, ActualWidth);
        var compositor = this.GetVisual().Compositor;
        var translation = forward
            ? - Children[ViewModel.CurrentLeadingIndex].DesiredSize.Width - ItemSpacing
            : Children[(ViewModel.CurrentLeadingIndex - 1 + childrenCount) % childrenCount].DesiredSize.Width + ItemSpacing;
        HashSet<int> animatedElementIndices = [];
        foreach (var (index, _) in _currentLayout)
        {
            animatedElementIndices.Add(index);
        }
        foreach (var (index, _) in updatedLayout)
        {
            animatedElementIndices.Add(index);
        }
        var animationEasingFunction = compositor.CreateCubicBezierEasingFunction(new(0.6f, 0f), new(0.4f, 1f));
        foreach (var index in animatedElementIndices)
        {
            var animation = compositor.CreateScalarKeyFrameAnimation();
            animation.Duration = TimeSpan.FromMilliseconds(1000);
            if (_currentLayout.Any(e => e.Index == index))
            {
                var element = _currentLayout.First(e => e.Index == index);
                animation.InsertKeyFrame(0, (float)element.Rect.X, animationEasingFunction);
                animation.InsertKeyFrame(1, (float)(element.Rect.X + translation), animationEasingFunction);
            }
            else
            {
                var element = updatedLayout.Single(e => e.Index == index);
                animation.InsertKeyFrame(0, (float)(element.Rect.X - translation), animationEasingFunction);
                animation.InsertKeyFrame(1, (float)element.Rect.X, animationEasingFunction);
            }
            Children[index].GetVisual().StartAnimation("Offset.X", animation);
        }
        for (var i = 0; i < childrenCount; i++)
        {
            if (animatedElementIndices.Contains(i)) continue;
            Children[i].GetVisual().Offset = new((float)(ActualWidth + ItemSpacing), 0, 0);
        }
        _currentLayout = updatedLayout;
        ViewModel.CurrentLeadingIndex = updatedLeadingIndex;
    }

    private (int Index, Rect Rect)[] ComputeLayout(int leadingIndex, double panelWidth)
    {
        if (Children.Count is 0) return Array.Empty<(int, Rect)>();
        var childrenCount = Children.Count;
        Rect leadingElementRect = new(new(MinimumEndPadding, 0), Children[leadingIndex].DesiredSize);
        List<(int Index, Rect Rect)> currentLayout = [(leadingIndex, leadingElementRect)];
        // Arrange left side.
        while (currentLayout[0].Rect.Left > 0)
        {
            var leftBoundIndex = (currentLayout[0].Index - 1 + childrenCount) % childrenCount;
            Rect leftRect = new(
                new(currentLayout[0].Rect.Left - Children[leftBoundIndex].DesiredSize.Width - ItemSpacing, 0),
                Children[leftBoundIndex].DesiredSize);
            currentLayout.Insert(0, (leftBoundIndex, leftRect));
        }
        // Arrange right side.
        while (currentLayout[^1].Rect.Right < panelWidth)
        {
            var rightBoundIndex = (currentLayout[^1].Index + 1) % childrenCount;
            Rect rightRect = new(new(currentLayout[^1].Rect.Right + ItemSpacing, 0),
                Children[rightBoundIndex].DesiredSize);
            currentLayout.Add((rightBoundIndex, rightRect));
        }
        return currentLayout.ToArray();
    }
}