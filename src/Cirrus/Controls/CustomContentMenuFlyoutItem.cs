using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace Cirrus.Controls;

[ContentProperty(Name = nameof(Content))]
public partial class CustomContentMenuFlyoutItem : MenuFlyoutItem
{
    public object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
        nameof(Content), typeof(object), typeof(CustomContentMenuFlyoutItem), new PropertyMetadata(null));

    public CustomContentMenuFlyoutItem()
    {
        DefaultStyleKey = typeof(CustomContentMenuFlyoutItem);
    }
}