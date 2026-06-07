using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Cirrus.Layout;

public sealed partial class Row : Panel
{
    public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(
        nameof(Spacing), typeof(double), typeof(Row), new PropertyMetadata(0d, OnSpacingChanged));

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    private static void OnSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Row row) return;
        row.InvalidateMeasure();
    }

    public static GridLength GetColumnWidth(DependencyObject obj) => (GridLength)obj.GetValue(ColumnWidthProperty);

    public static void SetColumnWidth(DependencyObject obj, GridLength value) =>
        obj.SetValue(ColumnWidthProperty, value);

    public static readonly DependencyProperty ColumnWidthProperty = DependencyProperty.RegisterAttached(
        "ColumnWidth", typeof(GridLength), typeof(Row), new PropertyMetadata(GridLength.Auto, OnChildLayoutChanged));

    public static int GetPriority(DependencyObject obj) => (int)obj.GetValue(PriorityProperty);
    public static void SetPriority(DependencyObject obj, int value) => obj.SetValue(PriorityProperty, value);

    public static readonly DependencyProperty PriorityProperty = DependencyProperty.RegisterAttached(
        "Priority", typeof(int), typeof(Row), new PropertyMetadata(0, OnChildLayoutChanged));

    private static void OnChildLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FrameworkElement child &&
            VisualTreeHelper.GetParent(child) is Row row)
        {
            row.InvalidateMeasure();
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        foreach (var hiddenChild in Children.Where(c => c.Visibility == Visibility.Collapsed))
        {
            hiddenChild.Measure(availableSize);
        }

        var activeChildren = Children.Where(c => c.Visibility == Visibility.Visible).ToList();
        if (activeChildren.Count == 0) return new Size(0, 0);

        double totalSpacing = (activeChildren.Count - 1) * Spacing;

        double widthLeft = double.IsInfinity(availableSize.Width)
            ? double.PositiveInfinity
            : Math.Max(0, availableSize.Width - totalSpacing);

        double maxHeight = 0;
        double consumedWidth = totalSpacing;

        var childInfos = activeChildren.Select(child => (
            Element: child,
            Width: GetColumnWidth(child),
            Priority: GetPriority(child)
        )).ToList();

        foreach (var info in childInfos.Where(c => c.Width.IsAbsolute).OrderByDescending(c => c.Priority))
        {
            info.Element.Measure(new Size(info.Width.Value, availableSize.Height));
            if (!double.IsInfinity(widthLeft)) widthLeft = Math.Max(0, widthLeft - info.Width.Value);
            consumedWidth += info.Width.Value;
            maxHeight = Math.Max(maxHeight, info.Element.DesiredSize.Height);
        }

        foreach (var info in childInfos.Where(c => c.Width.IsAuto).OrderByDescending(c => c.Priority))
        {
            info.Element.Measure(new Size(widthLeft, availableSize.Height));
            double desiredW = info.Element.DesiredSize.Width;
            if (!double.IsInfinity(widthLeft)) widthLeft = Math.Max(0, widthLeft - desiredW);
            consumedWidth += desiredW;
            maxHeight = Math.Max(maxHeight, info.Element.DesiredSize.Height);
        }

        var starChildren = childInfos.Where(c => c.Width.IsStar).ToList();
        double totalStarWeights = starChildren.Sum(c => c.Width.Value);

        if (totalStarWeights > 0)
        {
            double unitWidth = double.IsInfinity(widthLeft) ? 0 : (widthLeft / totalStarWeights);
            foreach (var info in starChildren)
            {
                double starW = info.Width.Value * unitWidth;
                info.Element.Measure(new Size(starW, availableSize.Height));
                if (!double.IsInfinity(widthLeft)) widthLeft = Math.Max(0, widthLeft - starW);
                consumedWidth += starW;
                maxHeight = Math.Max(maxHeight, info.Element.DesiredSize.Height);
            }
        }

        return new Size(consumedWidth, maxHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var activeChildren = Children.Where(c => c.Visibility != Visibility.Collapsed).ToList();
        if (activeChildren.Count == 0) return finalSize;

        double totalSpacing = (activeChildren.Count - 1) * Spacing;
        double reservedWidth = totalSpacing;
        double totalStarWeights = 0;

        foreach (var child in activeChildren)
        {
            var gl = GetColumnWidth(child);
            if (gl.IsAbsolute) reservedWidth += gl.Value;
            else if (gl.IsAuto) reservedWidth += child.DesiredSize.Width;
            else if (gl.IsStar) totalStarWeights += gl.Value;
        }

        double unitWidth = totalStarWeights > 0 ? Math.Max(0, finalSize.Width - reservedWidth) / totalStarWeights : 0;
        double xOffset = 0;

        foreach (var child in activeChildren)
        {
            var gl = GetColumnWidth(child);
            double childWidth = 0;

            if (gl.IsAbsolute) childWidth = gl.Value;
            else if (gl.IsAuto) childWidth = child.DesiredSize.Width;
            else if (gl.IsStar) childWidth = gl.Value * unitWidth;

            child.Arrange(new Rect(xOffset, 0, childWidth, finalSize.Height));

            xOffset += childWidth + Spacing;
        }

        return finalSize;
    }
}