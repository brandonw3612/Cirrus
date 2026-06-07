using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Cirrus.Layout;

public sealed partial class Column : Panel
{
    public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(
        nameof(Spacing), typeof(double), typeof(Column), new PropertyMetadata(0.0, OnSpacingChanged));

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    private static void OnSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((Column)d).InvalidateMeasure();
    }

    public static GridLength GetRowHeight(DependencyObject obj) => (GridLength)obj.GetValue(RowHeightProperty);
    public static void SetRowHeight(DependencyObject obj, GridLength value) => obj.SetValue(RowHeightProperty, value);

    public static readonly DependencyProperty RowHeightProperty = DependencyProperty.RegisterAttached(
        "RowHeight", typeof(GridLength), typeof(Column), new PropertyMetadata(GridLength.Auto));

    public static int GetPriority(DependencyObject obj) => (int)obj.GetValue(PriorityProperty);
    public static void SetPriority(DependencyObject obj, int value) => obj.SetValue(PriorityProperty, value);

    public static readonly DependencyProperty PriorityProperty = DependencyProperty.RegisterAttached(
        "Priority", typeof(int), typeof(Column), new PropertyMetadata(0));

    protected override Size MeasureOverride(Size availableSize)
    {
        var activeChildren = Children.Where(c => c.Visibility != Visibility.Collapsed).ToList();
        if (activeChildren.Count == 0) return new Size(0, 0);

        double totalSpacing = (activeChildren.Count - 1) * Spacing;

        double heightLeft = double.IsInfinity(availableSize.Height)
            ? double.PositiveInfinity
            : Math.Max(0, availableSize.Height - totalSpacing);

        double maxWidth = 0;
        double consumedHeight = totalSpacing;

        var childInfos = activeChildren
            .Select(child => (Element: child, Height: GetRowHeight(child), Priority: GetPriority(child))).ToList();

        foreach (var info in childInfos.Where(c => c.Height.IsAbsolute).OrderByDescending(c => c.Priority))
        {
            info.Element.Measure(new Size(availableSize.Width, info.Height.Value));
            if (!double.IsInfinity(heightLeft)) heightLeft = Math.Max(0, heightLeft - info.Height.Value);
            consumedHeight += info.Height.Value;
            maxWidth = Math.Max(maxWidth, info.Element.DesiredSize.Width);
        }

        foreach (var info in childInfos.Where(c => c.Height.IsAuto).OrderByDescending(c => c.Priority))
        {
            info.Element.Measure(new Size(availableSize.Width, heightLeft));
            double desiredH = info.Element.DesiredSize.Height;
            if (!double.IsInfinity(heightLeft)) heightLeft = Math.Max(0, heightLeft - desiredH);
            consumedHeight += desiredH;
            maxWidth = Math.Max(maxWidth, info.Element.DesiredSize.Width);
        }

        var starChildren = childInfos.Where(c => c.Height.IsStar).ToList();
        double totalStarWeights = starChildren.Sum(c => c.Height.Value);

        if (totalStarWeights > 0)
        {
            double unitHeight = double.IsInfinity(heightLeft) ? 0 : (heightLeft / totalStarWeights);
            foreach (var info in starChildren)
            {
                double starH = info.Height.Value * unitHeight;
                info.Element.Measure(new Size(availableSize.Width, starH));
                consumedHeight += starH;
                maxWidth = Math.Max(maxWidth, info.Element.DesiredSize.Width);
            }
        }

        return new Size(maxWidth, consumedHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var activeChildren = Children.Where(c => c.Visibility != Visibility.Collapsed).ToList();
        if (activeChildren.Count == 0) return finalSize;

        double totalSpacing = (activeChildren.Count - 1) * Spacing;
        double reservedHeight = totalSpacing;
        double totalStarWeights = 0;

        foreach (var child in activeChildren)
        {
            var gl = GetRowHeight(child);
            if (gl.IsAbsolute) reservedHeight += gl.Value;
            else if (gl.IsAuto) reservedHeight += child.DesiredSize.Height;
            else if (gl.IsStar) totalStarWeights += gl.Value;
        }

        double unitHeight =
            totalStarWeights > 0 ? Math.Max(0, finalSize.Height - reservedHeight) / totalStarWeights : 0;

        double yOffset = 0;

        foreach (var child in activeChildren)
        {
            var gl = GetRowHeight(child);
            double childHeight = 0;

            if (gl.IsAbsolute) childHeight = gl.Value;
            else if (gl.IsAuto) childHeight = child.DesiredSize.Height;
            else if (gl.IsStar) childHeight = gl.Value * unitHeight;

            child.Arrange(new Rect(0, yOffset, finalSize.Width, childHeight));

            yOffset += childHeight + Spacing;
        }

        return finalSize;
    }
}