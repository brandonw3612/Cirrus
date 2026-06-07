using Microsoft.UI.Xaml;

namespace Cirrus.Layout;

public sealed partial class Spacer : FrameworkElement
{
    public Spacer()
    {
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;
    }
}