using System.Numerics;
using System.Reactive.Linq;
using Windows.Foundation;
using Cirrus.Behaviors;
using Cirrus.Extensions;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;


namespace Cirrus.Controls;

public partial class GlidyScroller : Panel
{
    #region Dependency Properties

    public UIElement? FloatingLayer
    {
        get => (UIElement?)GetValue(FloatingLayerProperty);
        set => SetValue(FloatingLayerProperty, value);
    }
    public static readonly DependencyProperty FloatingLayerProperty =
        DependencyProperty.Register(nameof(FloatingLayer), typeof(UIElement), typeof(GlidyScroller), new PropertyMetadata(null, OnFloatingLayerChanged));

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
        nameof(Orientation), typeof(Orientation), typeof(GlidyScroller), new(Orientation.Vertical, OnOrientationChanged));

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(
        nameof(Spacing), typeof(double), typeof(GlidyScroller), new(0d, OnSpacingChanged));

    public int HighlightedChildIndex
    {
        get => (int)GetValue(HighlightedChildIndexProperty);
        set => SetValue(HighlightedChildIndexProperty, value);
    }
    
    public static readonly DependencyProperty HighlightedChildIndexProperty = DependencyProperty.Register(
        nameof(HighlightedChildIndex), typeof(int), typeof(GlidyScroller), new(0, OnHighlightedChildIndexChanged));

    #endregion

    #region Attached Properties
    
    public static readonly DependencyProperty ScaleFactorProperty = DependencyProperty.RegisterAttached("ScaleFactor",
        typeof(double), typeof(GlidyScroller), new(1d));
    public static double GetScaleFactor(DependencyObject obj) => (double)obj.GetValue(ScaleFactorProperty);
    public static void SetScaleFactor(DependencyObject obj, double value) => obj.SetValue(ScaleFactorProperty, value);

    #endregion

    private readonly List<UIElement> _items = [];
    public IReadOnlyList<UIElement> Items => _items;

    public void AppendItem(UIElement newItem)
    {
        _items.Add(newItem);
        Children.Add(newItem);
    }

    public void ClearItems()
    {
        foreach (var item in _items)
        {
            Children.Remove(item);
        }
        _items.Clear();
    }

    public double ContentLength { get; private set; }
    
    private readonly List<float> _childOffsets = [];
    private float _currentOffset;

    private IDisposable? _pwcDebouncerSubscription;
    private bool _isInteracting;

    public event EventHandler? UserInteractionStarted;
    public event EventHandler? UserInteractionEnded;

    public GlidyScroller()
    {
        this.AttachEventTriggerBehaviorToCommand<FrameworkElement, LoadedEventTriggerBehavior>(LoadedCommand);
        this.AttachEventTriggerBehaviorToCommand<FrameworkElement, UnloadedEventTriggerBehavior>(UnloadedCommand);
        this.AttachEventTriggerBehaviorToCommand<UIElement, PointerWheelChangedEventTriggerBehavior>
            (PointerWheelChangedCommand);
        // TODO: Manipulation event handlers
    }

    [RelayCommand]
    private void OnLoaded()
    {
        _pwcDebouncerSubscription = Observable.FromEventPattern<PointerEventHandler, PointerRoutedEventArgs>(
            h => PointerWheelChanged += h,
            h => PointerWheelChanged -= h
        ).Throttle(TimeSpan.FromMilliseconds(2000))
        .ObserveOn(DispatcherQueue)
        .Subscribe(_ =>
        {
            _isInteracting = false;
            UserInteractionEnded?.Invoke(this, EventArgs.Empty);
        });
    }

    [RelayCommand]
    private void OnUnloaded()
    {
        _pwcDebouncerSubscription?.Dispose();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var availableWidth = availableSize.Width;
        var availableHeight = availableSize.Height;

        if (Orientation is Orientation.Vertical) availableHeight = double.PositiveInfinity;
        else availableWidth = double.PositiveInfinity;

        if (_childOffsets.Count != _items.Count) HighlightedChildIndex = -1;
        
        _childOffsets.Clear();
        var offset = 0f;
        var elemSpacing = (float)Spacing;
        
        foreach (var child in _items)
        {
            var scaleFactor = GetScaleFactor(child);
            Size childAvailableSize = Orientation is Orientation.Vertical
                ? new(availableWidth / scaleFactor, availableHeight)
                : new(availableWidth, availableHeight / scaleFactor);
            child.Measure(childAvailableSize);
            var childDesiredSize = child.DesiredSize;
            _childOffsets.Add(offset);
            offset += Orientation is Orientation.Vertical
                ? (float)(childDesiredSize.Height * scaleFactor + elemSpacing)
                : (float)(childDesiredSize.Width * scaleFactor + elemSpacing);
        }

        ContentLength = offset;
        if (_items.Count > 0) ContentLength -= elemSpacing;

        if (HighlightedChildIndex >= 0 && HighlightedChildIndex < _items.Count)
        {
            var controlBound = Orientation is Orientation.Vertical ? (float)availableSize.Height : (float)availableSize.Width;
            _currentOffset = (float)Math.Clamp(
                _childOffsets[HighlightedChildIndex] - controlBound * 0.382f,
                0,
                ContentLength - controlBound
            );
        }

        if (FloatingLayer is { } fl)
        {
            fl.Measure(availableSize);
        }
        
        return ContentLength > 0 ? availableSize : new(0d, 0d);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (ContentLength == 0) return finalSize;
        var controlBound = Orientation is Orientation.Vertical ? finalSize.Height : finalSize.Width;
        _currentOffset = (float)Math.Clamp(_currentOffset, 0, Math.Max(0, ContentLength - controlBound));

        var visual = ElementCompositionPreview.GetElementVisual(this);
        var compositor = visual.Compositor;
        visual.Clip = compositor.CreateInsetClip();
        
        for (var i = 0; i < _items.Count; i++)
        {
            var child = _items[i];
            
            Vector3 position = Orientation is Orientation.Vertical
                ? new(0, _childOffsets[i] - _currentOffset, 0)
                : new(_childOffsets[i] - _currentOffset, 0, 0);
            
            child.Arrange(new(new Point(0, 0), child.DesiredSize));

            var childVisual = ElementCompositionPreview.GetElementVisual(child);
            childVisual.Offset = position;
        }

        if (FloatingLayer is { } fl)
        {
            fl.Arrange(new(
                new(finalSize._width - fl.DesiredSize._width, finalSize._height - fl.DesiredSize._height),
                fl.DesiredSize
            ));
        }

        return finalSize;
    }

    public float GetElementOffset(float idx)
    {
        var realIdx = (int)Math.Ceiling(idx * 2);
        if (realIdx < 0) return 0;
        if (realIdx / 2 >= _childOffsets.Count) return (float)ContentLength;
        return _childOffsets[realIdx / 2] + _items[realIdx / 2].ActualSize.Y * 0.5f;
    }

    private static void OnFloatingLayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not GlidyScroller scroller) return;
        if (e.OldValue is UIElement oldElement) scroller.Children.Remove(oldElement);
        if (e.NewValue is UIElement newElement)
        {
            scroller.Children.Add(newElement);
            Canvas.SetZIndex(newElement, 10);
        }
    }
    
    private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        (d as GlidyScroller)?.UpdateLayout();
    }
    
    private static void OnSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        (d as GlidyScroller)?.UpdateLayout();
    }
    
    private static void OnHighlightedChildIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not GlidyScroller scroller ||
            e.NewValue is not int newIdx ||
            scroller._childOffsets.Count != scroller._items.Count ||
            newIdx < 0 || newIdx >= scroller._items.Count ||
            scroller.ContentLength <= 0) return;

        var controlBound = scroller.Orientation is Orientation.Vertical
            ? scroller.ActualSize.Y
            : scroller.ActualSize.X;
        var actualOffset = (float)Math.Clamp(
            scroller._childOffsets[newIdx] + scroller._items[newIdx].ActualSize.Y * 0.5f - controlBound * 0.382f,
            0f,
            Math.Max(scroller.ContentLength - controlBound, 0)
        );
        if (Math.Abs(actualOffset - scroller._currentOffset) < 1e-3) return;

        var scrollingUpward = scroller._currentOffset < actualOffset;
        var delay = Math.Abs(actualOffset - scroller._currentOffset) < controlBound;
        for (var i = 0; i < scroller._items.Count; i++)
        {
            var visual = ElementCompositionPreview.GetElementVisual(scroller._items[i]);
            var offsetAnimation = visual.Compositor.CreateScalarKeyFrameAnimation();
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(1000);
            offsetAnimation.InsertKeyFrame(1f, scroller._childOffsets[i] - actualOffset);
            if (delay && ((scrollingUpward && i > newIdx) || (!scrollingUpward && i < newIdx)))
            {
                offsetAnimation.DelayTime = TimeSpan.FromMilliseconds(60 * Math.Abs(i - newIdx));
            }
            visual.StartAnimation(scroller.Orientation is Orientation.Vertical ? "Offset.Y" : "Offset.X",
                offsetAnimation);
        }

        scroller._currentOffset = actualOffset;
    }

    [RelayCommand]
    private void OnPointerWheelChanged(PointerRoutedEventArgs args)
    {
        if (!_isInteracting)
        {
            _isInteracting = true;
            UserInteractionStarted?.Invoke(this, EventArgs.Empty);
        }
        
        var actualOffset = _currentOffset - args.GetCurrentPoint(this).Properties.MouseWheelDelta;
        var controlBound = Orientation is Orientation.Vertical ? ActualHeight : ActualWidth;

        if (actualOffset > ContentLength - controlBound) actualOffset = (float)ContentLength - (float)controlBound;
        if (actualOffset < 0) actualOffset = 0;

        var animationTargetProperty = Orientation is Orientation.Vertical ? "Offset.Y" : "Offset.X";

        for (var i = 0; i < _items.Count; i++)
        {
            var visual = ElementCompositionPreview.GetElementVisual(_items[i]);
            var animation = visual.Compositor.CreateScalarKeyFrameAnimation();
            animation.Duration = TimeSpan.FromMilliseconds(200);
            animation.InsertKeyFrame(1f, _childOffsets[i] - actualOffset);
            visual.StopAnimation(animationTargetProperty);
            visual.StartAnimation(animationTargetProperty, animation);
        }

        _currentOffset = actualOffset;
    }
}