using Windows.Foundation;
using Microsoft.UI.Xaml.Controls;

namespace Cirrus.Controls;

public partial class SquaredPanel : Panel
{
    protected override Size MeasureOverride(Size availableSize)
    {
        var minEdge = Math.Min(availableSize.Width, availableSize.Height);
        Size size = new(minEdge, minEdge);
        if (Children.Count == 0) return size;
        Children[0].Measure(size);
        return size;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Children.Count == 0) return finalSize;
        var x = (finalSize.Width - Children[0].DesiredSize.Width) * 0.5d;
        var y = (finalSize.Height - Children[0].DesiredSize.Height) * 0.5d;
        Children[0].Arrange(new Rect(x, y, Children[0].DesiredSize.Width, Children[0].DesiredSize.Height));
        return finalSize;
    }
}