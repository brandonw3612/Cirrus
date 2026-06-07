using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Cirrus.Controls;

public sealed partial class SidebarMenuButton : Button
{
    public ImageSource? MenuIconSource
    {
        get => (ImageSource)GetValue(MenuIconSourceProperty);
        set => SetValue(MenuIconSourceProperty, value);
    }
    public static readonly DependencyProperty MenuIconSourceProperty =
        DependencyProperty.Register(nameof(MenuIconSource), typeof(ImageSource), typeof(SidebarMenuButton), new(null));

    public string Caption
    {
        get => (string)GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }
    public static readonly DependencyProperty CaptionProperty =
        DependencyProperty.Register(nameof(Caption), typeof(string), typeof(SidebarMenuButton), new(string.Empty));

    public double IconWidth
    {
        get => (double)GetValue(IconWidthProperty);
        set => SetValue(IconWidthProperty, value);
    }
    public static readonly DependencyProperty IconWidthProperty =
        DependencyProperty.Register(nameof(IconWidth), typeof(double), typeof(SidebarMenuButton), new(24));

    public double IconHeight
    {
        get => (double)GetValue(IconHeightProperty);
        set => SetValue(IconHeightProperty, value);
    }
    public static readonly DependencyProperty IconHeightProperty =
        DependencyProperty.Register(nameof(IconHeight), typeof(double), typeof(SidebarMenuButton), new(24));

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }
    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(SidebarMenuButton), new PropertyMetadata(false, OnIsSelectedChanged));

    public SidebarMenuButton()
    {
        DefaultStyleKey = typeof(SidebarMenuButton);
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not SidebarMenuButton sidebarMenuButton ||
            e.NewValue is not bool isSelected) return;
        VisualStateManager.GoToState(sidebarMenuButton, isSelected ? "Selected" : "Unselected", true);
    }
}