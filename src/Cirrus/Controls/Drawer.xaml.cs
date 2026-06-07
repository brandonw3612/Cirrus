using Cirrus.Views;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Animations;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;

namespace Cirrus.Controls;

public sealed partial class Drawer
{
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }
    private static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(Drawer), new PropertyMetadata(false, OnToggled));

    private static void OnToggled(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Drawer drawer ||
            e.OldValue == e.NewValue ||
            e.NewValue is not bool isOpen ||
            MainWindow.Current?.Compositor is not { } compositor) return;
        var animation = compositor.CreateVector3KeyFrameAnimation();
        var easingFunc = compositor.CreateCubicBezierEasingFunction(
            isOpen ? 0.4f : 0.6f, 0f, 0.4f, 1f
        );
        animation.InsertKeyFrame(1f, new(0f, isOpen ? 0f : (float)drawer.ActualHeight, 0f), easingFunc);
        var visual = ElementCompositionPreview.GetElementVisual(drawer.ContentFrame);
        var ratio = visual.Offset.Y / drawer.ActualHeight;
        if (!isOpen) ratio = 1d - ratio;
        animation.Duration = TimeSpan.FromMilliseconds(Math.Max(500 * ratio, 200));
        visual.StartAnimation(nameof(visual.Offset), animation);
    }

    public Drawer()
    {
        InitializeComponent();
    }

    public void Navigate(Type pageType)
    {
        ContentFrame.Navigate(pageType);
    }

    [RelayCommand]
    private void OnSizeChanged(SizeChangedEventArgs e)
    {
        if (IsOpen) return;
        var visual = ElementCompositionPreview.GetElementVisual(ContentFrame);
        visual.Offset = new(0f, (float)e.NewSize.Height, 0f);
    }
}
