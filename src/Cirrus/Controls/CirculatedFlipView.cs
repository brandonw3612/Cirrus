using System.Collections;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Cirrus.Behaviors;
using Cirrus.Extensions;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using WinRT;

namespace Cirrus.Controls;

[GeneratedBindableCustomProperty([
    nameof(ScrollBackwardCommand),
    nameof(ScrollForwardCommand)
], [])]
public sealed partial class CirculatedFlipView : Control
{
    public object? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(object), typeof(CirculatedFlipView),
            new PropertyMetadata(null, OnItemsChanged));
    
    public IElementFactory? ItemTemplate
    {
        get => (IElementFactory)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.Register(nameof(ItemTemplate), typeof(IElementFactory), typeof(CirculatedFlipView),
            new PropertyMetadata(null, OnItemsChanged));
    
    public double ItemSpacing
    {
        get => (double)GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    public static readonly DependencyProperty ItemSpacingProperty =
        DependencyProperty.Register(nameof(ItemSpacing), typeof(double), typeof(CirculatedFlipView),
            new PropertyMetadata(0));

    public double MinimumEndPadding
    {
        get => (double)GetValue(MinimumEndPaddingProperty);
        set => SetValue(MinimumEndPaddingProperty, value);
    }

    public static readonly DependencyProperty MinimumEndPaddingProperty =
        DependencyProperty.Register(nameof(MinimumEndPadding), typeof(double), typeof(CirculatedFlipView),
            new PropertyMetadata(0));

    private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CirculatedFlipView view) return;
        view.UpdateElements();
    }

    private readonly IDisposable _translationSubscription;
    private readonly Subject<bool> _translationSubject;
    
    private readonly Timer _autoScrollingTimer;

    public CirculatedFlipView()
    {
        DefaultStyleKey = typeof(CirculatedFlipView);
        this.AttachEventTriggerBehaviorToCommand<FrameworkElement, UnloadedEventTriggerBehavior>(UnloadedCommand);
        ManipulationMode = ManipulationModes.TranslateX;
        _translationSubject = new();
        _translationSubscription = _translationSubject
            .GroupByUntil(
                keySelector: _ => 1,
                durationSelector: _ => Observable.Timer(TimeSpan.FromMilliseconds(1000))
            ).SelectMany(g => g.Take(1))
            .ObserveOn(DispatcherQueue)
            .Subscribe(forward => _viewPanel?.Translate(forward));
        _autoScrollingTimer = new(_ =>
        {
            DispatcherQueue.TryEnqueue(() => _viewPanel?.Translate(true));
        }, null, TimeSpan.FromMilliseconds(5000), TimeSpan.FromMilliseconds(5000));
    }

    private CirculatedFlipViewPanel? _viewPanel;

    [RelayCommand]
    private void OnUnloaded()
    {
        _autoScrollingTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        _translationSubscription.Dispose();
        _translationSubject.Dispose();
    }
    
    protected override void OnApplyTemplate()
    {
        _viewPanel = GetTemplateChild("ViewPanel") as CirculatedFlipViewPanel;
        UpdateElements();
        var visual = this.GetVisual();
        var insetClip = visual.Compositor.CreateInsetClip();
        visual.Clip = insetClip;
        base.OnApplyTemplate();
    }

    private void UpdateElements()
    {
        if (_viewPanel is null) return;
        _viewPanel.Children.Clear();
        if (ItemsSource is not IEnumerable itemsSource ||
            itemsSource.OfType<object>().ToArray() is not { Length: > 0 } items ||
            ItemTemplate is null) return;
        foreach (var item in items)
        {
            var element = ItemTemplate.GetElement(new()
            {
                Data = item,
                Parent = _viewPanel
            });
            element.SetValue(DataContextProperty, item);
            _viewPanel.Children.Add(element);
        }
        _viewPanel.UpdateLayout();
    }

    protected override void OnPointerEntered(PointerRoutedEventArgs e)
    {
        _autoScrollingTimer.Change(Timeout.InfiniteTimeSpan, TimeSpan.FromMilliseconds(5000));
    }

    protected override void OnPointerExited(PointerRoutedEventArgs e)
    {
        _autoScrollingTimer.Change(TimeSpan.FromMilliseconds(5000), TimeSpan.FromMilliseconds(5000));
    }

    protected override void OnPointerPressed(PointerRoutedEventArgs e)
    {
        CapturePointer(e.Pointer);
    }

    protected override void OnPointerReleased(PointerRoutedEventArgs e)
    {
        ReleasePointerCapture(e.Pointer);
    }

    protected override void OnPointerWheelChanged(PointerRoutedEventArgs e)
    {
        if (_viewPanel is null) return;
        var properties = e.GetCurrentPoint(this).Properties;
        var delta = properties.MouseWheelDelta * (properties.IsHorizontalMouseWheel ? -1 : 1);
        switch (delta)
        {
            case >= 120:
                _translationSubject.OnNext(false);
                break;
            case <= -120:
                _translationSubject.OnNext(true);
                break;
        }
    }

    protected override void OnManipulationDelta(ManipulationDeltaRoutedEventArgs e)
    {
        if (_viewPanel is null) return;
        var delta = e.Delta.Translation.X;
        switch (delta)
        {
            case >= 20:
                _translationSubject.OnNext(false);
                break;
            case <= -20:
                _translationSubject.OnNext(true);
                break;
        }
    }

    public RelayCommand? ScrollBackwardCommand => field ??= new RelayCommand(ScrollBackward);
    private void ScrollBackward() => _translationSubject.OnNext(false);

    public RelayCommand? ScrollForwardCommand => field ??= new RelayCommand(ScrollForward);
    private void ScrollForward() => _translationSubject.OnNext(true);
}