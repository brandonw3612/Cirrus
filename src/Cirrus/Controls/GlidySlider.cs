using Windows.Foundation;
using Windows.UI;
using Cirrus.Behaviors;
using Cirrus.Extensions;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.UI.Composition;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;

namespace Cirrus.Controls;

public sealed partial class GlidySlider : RangeBase
{
    private Compositor? _compositor;
    private CompositionRoundedRectangleGeometry? _clipGeometry;
    private SpriteVisual? _rootVisual, _containerVisual, _foregroundVisual;
    private CompositionColorBrush? _backgroundBrush, _foregroundBrush;

    private bool _isPointerPressed;

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(GlidySlider),
            new PropertyMetadata(Orientation.Horizontal, OnOrientationChanged));

    public new double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public new static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(double), typeof(GlidySlider),
            new PropertyMetadata(-1d, OnCornerRadiusChanged));

    public double AnimationScaleFactor
    {
        get => (double)GetValue(AnimationScaleFactorProperty);
        set => SetValue(AnimationScaleFactorProperty, value);
    }

    public static readonly DependencyProperty AnimationScaleFactorProperty =
        DependencyProperty.Register(nameof(AnimationScaleFactor), typeof(double), typeof(GlidySlider),
            new PropertyMetadata(2d, OnAnimationScaleFactorChanged));

    public bool UpdateOnInteraction
    {
        get => (bool)GetValue(UpdateOnInteractionProperty);
        set => SetValue(UpdateOnInteractionProperty, value);
    }

    public static readonly DependencyProperty UpdateOnInteractionProperty =
        DependencyProperty.Register(nameof(UpdateOnInteraction), typeof(bool), typeof(GlidySlider),
            new PropertyMetadata(false));

    private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not GlidySlider { _foregroundVisual: { } foregroundVisual } slider ||
            e.NewValue is not Orientation orientation) return;
        if (slider._compositor is null) return;
        var ratio = Math.Abs(slider.Maximum - slider.Minimum) < 1e-3
            ? 0f
            : Math.Clamp((float)((slider.Value - slider.Minimum) / (slider.Maximum - slider.Minimum)), 0f, 1f);
        foregroundVisual.RelativeSizeAdjustment = orientation is Orientation.Horizontal
            ? new(ratio, 1f)
            : new(1f, ratio);
        foregroundVisual.RelativeOffsetAdjustment = orientation is Orientation.Horizontal
            ? new(0f, 0f, 0f)
            : new(0f, 1f - ratio, 0f);
    }

    private static void OnCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not GlidySlider { _clipGeometry: { } clipGeometry } slider || e.NewValue is not double newRadius) return;
        if (newRadius < 0d)
        {
            double focusedWidth = slider.ActualWidth, focusedHeight = slider.ActualHeight;
            var unfocusedWidth = slider.Orientation is Orientation.Horizontal
                ? Math.Max(focusedWidth - focusedHeight + focusedHeight / slider.AnimationScaleFactor, 0f)
                : focusedWidth / slider.AnimationScaleFactor;
            var unfocusedHeight = slider.Orientation is Orientation.Horizontal
                ? focusedHeight / slider.AnimationScaleFactor
                : Math.Max(focusedHeight - focusedWidth + focusedWidth / slider.AnimationScaleFactor, 0f);
            newRadius = Math.Min(unfocusedWidth, unfocusedHeight) * 0.5f;
        }
        clipGeometry.CornerRadius = new((float)newRadius);
    }
    
    private static void OnAnimationScaleFactorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not GlidySlider { _containerVisual: { } containerVisual, _clipGeometry: { } clipGeometry } slider ||
            e.NewValue is not double newScaleFactor ||
            newScaleFactor < 1d) return;
        double focusedWidth = slider.ActualWidth, focusedHeight = slider.ActualHeight;
        var unfocusedWidth = slider.Orientation is Orientation.Horizontal
            ? Math.Max(focusedWidth - focusedHeight + focusedHeight / newScaleFactor, 0d)
            : focusedWidth / newScaleFactor;
        var unfocusedHeight = slider.Orientation is Orientation.Horizontal
            ? focusedHeight / newScaleFactor
            : Math.Max(focusedHeight - focusedWidth + focusedWidth / newScaleFactor, 0d);
        var unfocusedOffset = slider.Orientation is Orientation.Horizontal
            ? (focusedHeight - unfocusedHeight) * 0.5d
            : (focusedWidth - unfocusedWidth) * 0.5d;
        var cornerRadius = (float)slider.CornerRadius;
        if (cornerRadius < 0f) cornerRadius = (float)Math.Min(unfocusedWidth, unfocusedHeight) * 0.5f;
        containerVisual.Size = new((float)unfocusedWidth, (float)unfocusedHeight);
        containerVisual.Offset = new((float)unfocusedOffset, (float)unfocusedOffset, 0);
        clipGeometry.Size = new((float)unfocusedWidth, (float)unfocusedHeight);
        clipGeometry.CornerRadius = new(cornerRadius);
    }
    
    public GlidySlider()
    {
        DefaultStyleKey = typeof(GlidySlider);
        this.AttachEventTriggerBehaviorToCommand<FrameworkElement, ActualThemeChangedEventTriggerBehavior>
            (ThemeChangedCommand);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return availableSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (_rootVisual is null || _containerVisual is null || _clipGeometry is null)
            return finalSize;
        float focusedWidth = (float)finalSize.Width, focusedHeight = (float)finalSize.Height;
        var unfocusedWidth = Orientation is Orientation.Horizontal
            ? Math.Max(focusedWidth - focusedHeight + focusedHeight / (float)AnimationScaleFactor, 0f)
            : focusedWidth / (float)AnimationScaleFactor;
        var unfocusedHeight = Orientation is Orientation.Horizontal
            ? focusedHeight / (float)AnimationScaleFactor
            : Math.Max(focusedHeight - focusedWidth + focusedWidth / (float)AnimationScaleFactor, 0f);
        var unfocusedOffset = Orientation is Orientation.Horizontal
            ? (focusedHeight - unfocusedHeight) * 0.5f
            : (focusedWidth - unfocusedWidth) * 0.5f;
        var cornerRadius = (float)CornerRadius;
        if (cornerRadius < 0f) cornerRadius = Math.Min(unfocusedWidth, unfocusedHeight) * 0.5f;
        _rootVisual.Size = new(focusedWidth, focusedHeight);
        _containerVisual.Size = new(unfocusedWidth, unfocusedHeight);
        _containerVisual.Offset = new(unfocusedOffset, unfocusedOffset, 0);
        _clipGeometry.CornerRadius = new(cornerRadius);
        _clipGeometry.Size = new(unfocusedWidth, unfocusedHeight);
        (GetTemplateChild("RootBorder") as Border)?.Arrange(new(new(0f, 0f), finalSize));
        return finalSize;
    }
    
    [RelayCommand]
    private void OnThemeChanged()
    {
        if (_backgroundBrush is null || _foregroundBrush is null) return;
        _backgroundBrush.Color = ActualTheme is ElementTheme.Light
            ? Color.FromArgb(0x3F, 0xBF, 0xBF, 0xBF)
            : Color.FromArgb(0x3F, 0x50, 0x50, 0x50);
        _foregroundBrush.Color = ActualTheme is ElementTheme.Light
            ? Color.FromArgb(0xBF, 0x7F, 0x7F, 0x7F)
            : Color.FromArgb(0xBF, 0xFF, 0xFF, 0xFF);

    }

    protected override void OnApplyTemplate()
    {
        _compositor = this.GetVisual().Compositor;

        float focusedWidth = (float)ActualWidth, focusedHeight = (float)ActualHeight;
        var unfocusedWidth = Orientation is Orientation.Horizontal
            ? Math.Max(focusedWidth - focusedHeight + focusedHeight / (float)AnimationScaleFactor, 0f)
            : focusedWidth / (float)AnimationScaleFactor;
        var unfocusedHeight = Orientation is Orientation.Horizontal
            ? focusedHeight / (float)AnimationScaleFactor
            : Math.Max(focusedHeight - focusedWidth + focusedWidth / (float)AnimationScaleFactor, 0f);
        var unfocusedOffset = Orientation is Orientation.Horizontal
            ? (focusedHeight - unfocusedHeight) * 0.5f
            : (focusedWidth - unfocusedWidth) * 0.5f;
        var cornerRadius = (float)CornerRadius;
        if (cornerRadius < 0f) cornerRadius = Math.Min(unfocusedWidth, unfocusedHeight) * 0.5f;

        var ratio = Math.Abs(Maximum - Minimum) < 1e-3
            ? 0f
            : Math.Clamp((float)((Value - Minimum) / (Maximum - Minimum)), 0f, 1f);
        
        _rootVisual = _compositor.BuildSpriteVisual(
            size: new(focusedWidth, focusedHeight),
            children:
            [
                _compositor.BuildSpriteVisual(out _containerVisual,
                    size: new(unfocusedWidth, unfocusedHeight),
                    offset: new(unfocusedOffset, unfocusedOffset, 0),
                    children:
                    [
                        _compositor.BuildSpriteVisual(
                            relativeSizeAdjustment: new(1f, 1f),
                            brush: _compositor.BuildColorBrush(out _backgroundBrush,
                                ActualTheme is ElementTheme.Light
                                    ? Color.FromArgb(0x3F, 0xBF, 0xBF, 0xBF)
                                    : Color.FromArgb(0x3F, 0x50, 0x50, 0x50)
                            )
                        ),
                        _compositor.BuildSpriteVisual(out _foregroundVisual,
                            relativeSizeAdjustment: Orientation is Orientation.Horizontal
                                ? new(ratio, 1f)
                                : new(1f, ratio),
                            relativeOffsetAdjustment: Orientation is Orientation.Horizontal
                                ? new(0f, 0f, 0f)
                                : new(0f, 1f - ratio, 0f),
                            brush: _compositor.BuildColorBrush(out _foregroundBrush,
                                ActualTheme is ElementTheme.Light
                                    ? Color.FromArgb(0xBF, 0x7F, 0x7F, 0x7F)
                                    : Color.FromArgb(0xBF, 0xFF, 0xFF, 0xFF)
                            ),
                            opacity: 0.5f
                        )
                    ],
                    clip: _compositor.CreateGeometricClip(
                        geometry: _compositor.BuildRoundedRectangleGeometry(out _clipGeometry,
                            cornerRadius: new(cornerRadius),
                            size: new(unfocusedWidth, unfocusedHeight)
                        )
                    )
                )
            ]
        );
        ElementCompositionPreview.SetElementChildVisual(this, _rootVisual);

        base.OnApplyTemplate();
    }

    protected override void OnPointerEntered(PointerRoutedEventArgs e)
    {
        if (_compositor is null) return;
        float focusedWidth = (float)ActualWidth, focusedHeight = (float)ActualHeight;
        var cornerRadius = (float)CornerRadius;
        if (cornerRadius < 0f) cornerRadius = Math.Min(focusedWidth, focusedHeight) * 0.5f;
        _containerVisual!.StartAnimationGroup(_compositor.BuildAnimationGroupWith(
            _compositor.BuildVector2KeyFrameAnimation(
                finalValue: new(focusedWidth, focusedHeight),
                target: nameof(_containerVisual.Size)
            ),
            _compositor.BuildVector3KeyFrameAnimation(
                finalValue: new(0f),
                target: nameof(_containerVisual.Offset)
            )
        ));
        _clipGeometry!.StartAnimationGroup(_compositor.BuildAnimationGroupWith(
            _compositor.BuildVector2KeyFrameAnimation(
                finalValue: new(focusedWidth, focusedHeight),
                target: nameof(_clipGeometry.Size)
            ),
            _compositor.BuildVector2KeyFrameAnimation(
                finalValue: new(cornerRadius),
                target: nameof(_clipGeometry.CornerRadius)
            )
        ));
        _foregroundVisual!.StartAnimation(nameof(_foregroundVisual.Opacity),
            _compositor.BuildSpringScalarAnimation(
                finalValue: 1f
        ));
        base.OnPointerEntered(e);
    }

    protected override void OnPointerExited(PointerRoutedEventArgs e)
    {
        if (_compositor is null) return;
        float focusedWidth = (float)ActualWidth, focusedHeight = (float)ActualHeight;
        var unfocusedWidth = Orientation is Orientation.Horizontal
            ? Math.Max(focusedWidth - focusedHeight + focusedHeight / (float)AnimationScaleFactor, 0f)
            : focusedWidth / (float)AnimationScaleFactor;
        var unfocusedHeight = Orientation is Orientation.Horizontal
            ? focusedHeight / (float)AnimationScaleFactor
            : Math.Max(focusedHeight - focusedWidth + focusedWidth / (float)AnimationScaleFactor, 0f);
        var unfocusedOffset = Orientation is Orientation.Horizontal
            ? (focusedHeight - unfocusedHeight) * 0.5f
            : (focusedWidth - unfocusedWidth) * 0.5f;
        var cornerRadius = (float)CornerRadius;
        if (cornerRadius < 0f) cornerRadius = Math.Min(unfocusedWidth, unfocusedHeight) * 0.5f;

        _containerVisual!.StartAnimationGroup(_compositor.BuildAnimationGroupWith(
            _compositor.BuildVector2KeyFrameAnimation(
                finalValue: new(unfocusedWidth, unfocusedHeight),
                target: nameof(_containerVisual.Size)
            ),
            _compositor.BuildVector3KeyFrameAnimation(
                finalValue: new(unfocusedOffset, unfocusedOffset, 0f),
                target: nameof(_containerVisual.Offset)
            )
        ));
        _clipGeometry!.StartAnimationGroup(_compositor.BuildAnimationGroupWith(
            _compositor.BuildVector2KeyFrameAnimation(
                finalValue: new(unfocusedWidth, unfocusedHeight),
                target: nameof(_clipGeometry.Size)
            ),
            _compositor.BuildVector2KeyFrameAnimation(
                finalValue: new(cornerRadius),
                target: nameof(_clipGeometry.CornerRadius)
            )
        ));
        _foregroundVisual!.StartAnimation(nameof(_foregroundVisual.Opacity),
            _compositor.BuildSpringScalarAnimation(
                finalValue: 0.5f
        ));
        // ToolTipService.SetToolTip(this, null);
        base.OnPointerExited(e);
    }

    private double _manipulationRatioDelta;

    protected override void OnPointerPressed(PointerRoutedEventArgs e)
    {
        e.Handled = true;
        _isPointerPressed = true;
        if (_compositor is null) return;
        float focusedWidth = (float)ActualWidth, focusedHeight = (float)ActualHeight;
        var pointerPositionRatio = Orientation is Orientation.Horizontal
            ? e.GetCurrentPoint(this).Position.X / focusedWidth
            : 1f - e.GetCurrentPoint(this).Position.Y / focusedHeight;
        var currentRatio = Math.Abs(Maximum - Minimum) < 1e-3
            ? 0f
            : Math.Clamp((float)((Value - Minimum) / (Maximum - Minimum)), 0f, 1f);
        _manipulationRatioDelta = e.Pointer.PointerDeviceType is PointerDeviceType.Touch
            ? currentRatio - pointerPositionRatio
            : 0d;
        var valueRatio = Math.Clamp(pointerPositionRatio + _manipulationRatioDelta, 0d, 1d);
        
        _foregroundVisual!.StartAnimationGroup(_compositor.BuildAnimationGroupWith(
            _compositor.BuildSpringVector2Animation(
                finalValue: Orientation is Orientation.Horizontal
                    ? new((float)valueRatio, 1f)
                    : new(1f, (float)valueRatio),
                target: nameof(_foregroundVisual.RelativeSizeAdjustment)
            ),
            _compositor.BuildSpringVector3Animation(
                finalValue: Orientation is Orientation.Horizontal
                    ? new(0f, 0f, 0f)
                    : new(0f, 1f - (float)valueRatio, 0f),
                target: nameof(_foregroundVisual.RelativeOffsetAdjustment)
            )
        ));
        CapturePointer(e.Pointer);
        base.OnPointerPressed(e);
    }

    protected override void OnPointerMoved(PointerRoutedEventArgs e)
    {
        e.Handled = true;
        if (_isPointerPressed && _compositor is not null)
        {
            float focusedWidth = (float)ActualWidth, focusedHeight = (float)ActualHeight;
            var pointerPositionRatio = Orientation is Orientation.Horizontal
                ? e.GetCurrentPoint(this).Position.X / focusedWidth
                : 1f - e.GetCurrentPoint(this).Position.Y / focusedHeight;
            var ratio = Math.Clamp(pointerPositionRatio + _manipulationRatioDelta, 0d, 1d);
            _foregroundVisual!.RelativeSizeAdjustment = Orientation is Orientation.Horizontal
                ? new((float)ratio, 1f)
                : new(1f, (float)ratio);
            _foregroundVisual.RelativeOffsetAdjustment = Orientation is Orientation.Horizontal
                ? new(0f, 0f, 0f)
                : new(0f, 1f - (float)ratio, 0f);
            if (UpdateOnInteraction)
            {
                Value = Minimum + (Maximum - Minimum) * ratio;
            }
        }
        base.OnPointerMoved(e);
    }

    protected override void OnPointerReleased(PointerRoutedEventArgs e)
    {
        e.Handled = true;
        _isPointerPressed = false;
        if (_compositor is null) return;
        float focusedWidth = (float)ActualWidth, focusedHeight = (float)ActualHeight;
        var pointerPositionRatio = Orientation is Orientation.Horizontal
            ? e.GetCurrentPoint(this).Position.X / focusedWidth
            : 1f - e.GetCurrentPoint(this).Position.Y / focusedHeight;
        var ratio = Math.Clamp(pointerPositionRatio + _manipulationRatioDelta, 0d, 1d);
        Value = Minimum + (Maximum - Minimum) * ratio;
        ReleasePointerCapture(e.Pointer);
        base.OnPointerReleased(e);
    }

    protected override void OnMinimumChanged(double oldMinimum, double newMinimum)
    {
        UpdateForegroundRatio();
    }

    protected override void OnValueChanged(double oldValue, double newValue)
    {
        if (_isPointerPressed) return;
        UpdateForegroundRatio();
    }

    protected override void OnMaximumChanged(double oldMaximum, double newMaximum)
    {
        UpdateForegroundRatio();
    }

    private void UpdateForegroundRatio()
    {
        if (_compositor is null) return;
        var valueRatio = Math.Abs(Maximum - Minimum) < 1e-3 
            ? 0d
            : Math.Clamp((Value - Minimum) / (Maximum - Minimum), 0d, 1d);
        _foregroundVisual!.StartAnimationGroup(_compositor.BuildAnimationGroupWith(
            _compositor.BuildSpringVector2Animation(
                finalValue: Orientation is Orientation.Horizontal
                    ? new((float)valueRatio, 1f)
                    : new(1f, (float)valueRatio),
                target: nameof(_foregroundVisual.RelativeSizeAdjustment)
            ),
            _compositor.BuildSpringVector3Animation(
                finalValue: Orientation is Orientation.Horizontal
                    ? new(0f, 0f, 0f)
                    : new(0f, 1f - (float)valueRatio, 0f),
                target: nameof(_foregroundVisual.RelativeOffsetAdjustment)
            )
        ));
    }
}