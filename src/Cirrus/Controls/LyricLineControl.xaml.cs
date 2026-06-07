using System.Text;
using Cirrus.Base.Services;
using Cirrus.Behaviors;
using Cirrus.LiveModels;
using Cirrus.Models.Business.Lyrics;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Composition;
using Microsoft.UI.Input;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Colors = Microsoft.UI.Colors;

namespace Cirrus.Controls;

public sealed partial class LyricLineControl
{
    private record LyricFragmentRegion(int Line, double InlineLeft, double Width);

    private class LyricFragmentMapping(LyricFragment fragment, int textLength)
    {
        public List<LyricFragmentRegion> Regions { get; } = [];
        public LyricFragment Fragment { get; } = fragment;
        public int TextLength { get; } = textLength;

        public (int Line, double LeftWith) ComputeFraction(double frac)
        {
            frac = Math.Clamp(frac, 0d, 1d);
            var totalWidth = Regions.Sum(r => r.Width);
            var w = totalWidth * frac;
            foreach (var r in Regions)
            {
                if (w > r.Width) w -= r.Width;
                else return (r.Line, r.InlineLeft + w);
            }
            var lastLine = Regions.MaxBy(r => r.Line);
            if (lastLine is null) return (0, 0d);
            return (lastLine.Line, lastLine.InlineLeft + lastLine.Width);
        }
    }

    private record LineMapping(CompositionColorGradientStop GradientStart, CompositionColorGradientStop GradientEnd)
    {
        public double Width { get; set; }
    }

    public double PlaybackPosition
    {
        get => (double)GetValue(PlaybackPositionProperty);
        set => SetValue(PlaybackPositionProperty, value);
    }
    public static readonly DependencyProperty PlaybackPositionProperty =
        DependencyProperty.Register(nameof(PlaybackPosition), typeof(double), typeof(LyricLineControl), new PropertyMetadata(0, OnPlaybackPositionUpdated));

    public double BaseFontSize
    {
        get => (double)GetValue(BaseFontSizeProperty);
        set => SetValue(BaseFontSizeProperty, value);
    }
    public static readonly DependencyProperty BaseFontSizeProperty =
        DependencyProperty.Register(nameof(BaseFontSize), typeof(double), typeof(LyricLineControl), new PropertyMetadata(24d, OnFontSizeChanged));

    public bool IsTranslationEnabled
    {
        get => (bool)GetValue(IsTranslationEnabledProperty);
        set => SetValue(IsTranslationEnabledProperty, value);
    }
    public static readonly DependencyProperty IsTranslationEnabledProperty =
        DependencyProperty.Register(nameof(IsTranslationEnabled), typeof(bool), typeof(LyricLineControl), new PropertyMetadata(true, OnIsTranslationEnabledChanged));

    public bool IsScaled
    {
        get => (bool)GetValue(IsScaledProperty);
        set => SetValue(IsScaledProperty, value);
    }
    public static readonly DependencyProperty IsScaledProperty =
        DependencyProperty.Register(nameof(IsScaled), typeof(bool), typeof(LyricLineControl), new PropertyMetadata(false, OnIsScaledChanged));

    public bool IsPreviewed
    {
        get => (bool)GetValue(IsPreviewedProperty);
        set => SetValue(IsPreviewedProperty, value);
    }
    public static readonly DependencyProperty IsPreviewedProperty =
        DependencyProperty.Register(nameof(IsPreviewed), typeof(bool), typeof(LyricLineControl), new PropertyMetadata(false, OnIsPreviewedChanged));

    public double StaticClarity
    {
        get => (double)GetValue(StaticClarityProperty);
        set => SetValue(StaticClarityProperty, value);
    }
    public static readonly DependencyProperty StaticClarityProperty =
        DependencyProperty.Register(nameof(StaticClarity), typeof(double), typeof(LyricLineControl), new PropertyMetadata(1d, OnStaticClarityChanged));

    public string FallbackLine { get; }
    
    private readonly TimeSpan _start, _duration;
    private readonly string _translatedLine;
    private string _lineFullString;
    private readonly List<LyricFragmentMapping> _fragmentMappings;
    private readonly List<LineMapping> _lineMappings;
    
    private readonly UserPreferenceService _preference = ServicesProvider.Current.GetService<UserPreferenceService>()!;
    private readonly PlaybackServiceBridge _playbackServiceBridge;
    
    private readonly Visual _backgroundVisual;

    private readonly SemaphoreSlim _viewSemaphore;
    
    public LyricLineControl(LyricLine data, PlaybackServiceBridge playbackServiceBridge)
    {
        _playbackServiceBridge = playbackServiceBridge;
        
        _start = data.Start;
        _duration = data.Duration;
        FallbackLine = data.FallbackText;
        _translatedLine = data.Translation;
        _fragmentMappings = [];
        _lineFullString = string.Empty;
        BuildFragmentMappings(data);
        _lineMappings = [];
        
        InitializeComponent();
        _backgroundVisual = ElementCompositionPreview.GetElementVisual(BackgroundBorder);

        _viewSemaphore = new(1);

        B2BGrid.Visibility = data.B2BLyrics is { Count: 0 } ? Visibility.Collapsed : Visibility.Visible;
        FallbackLineTextBlock.Visibility = data.B2BLyrics is { Count: 0 } ? Visibility.Visible : Visibility.Collapsed;
        TranslationTextBlock.Visibility = (IsTranslationEnabled && data.Translation.Length > 0) ? Visibility.Visible : Visibility.Collapsed;
    }

    private static async void OnPlaybackPositionUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not LyricLineControl control ||
            e.NewValue is not double prog) return;
        if (!await control._viewSemaphore.WaitAsync(0)) return;
        prog += 200;
        try
        {
            if (prog < control._start.TotalMilliseconds)
            {
                foreach (var lm in control._lineMappings)
                {
                    var startAnimation = lm.GradientStart.Compositor.CreateScalarKeyFrameAnimation();
                    startAnimation.Duration = TimeSpan.FromMilliseconds(200);
                    startAnimation.InsertKeyFrame(1f, 0f);
                    var endAnimation = lm.GradientEnd.Compositor.CreateScalarKeyFrameAnimation();
                    endAnimation.Duration = TimeSpan.FromMilliseconds(200);
                    endAnimation.InsertKeyFrame(1f, 0f);
                    lm.GradientStart.StartAnimation(nameof(lm.GradientStart.Offset), startAnimation);
                    lm.GradientEnd.StartAnimation(nameof(lm.GradientEnd.Offset), endAnimation);
                }
                return;
            }
            if (prog >= (control._start + control._duration).TotalMilliseconds)
            {
                foreach (var lm in control._lineMappings)
                {
                    var startAnimation = lm.GradientStart.Compositor.CreateScalarKeyFrameAnimation();
                    startAnimation.Duration = TimeSpan.FromMilliseconds(200);
                    startAnimation.InsertKeyFrame(1f, 1f);
                    var endAnimation = lm.GradientEnd.Compositor.CreateScalarKeyFrameAnimation();
                    endAnimation.Duration = TimeSpan.FromMilliseconds(200);
                    endAnimation.InsertKeyFrame(1f, 1f);
                    lm.GradientStart.StartAnimation(nameof(lm.GradientStart.Offset), startAnimation);
                    lm.GradientEnd.StartAnimation(nameof(lm.GradientEnd.Offset), endAnimation);
                }
                return;
            }
            var currentIndex = control._fragmentMappings.FindLastIndex(m => m.Fragment.Start.TotalMilliseconds <= prog);
            if (currentIndex >= 0)
            {
                var mapping = control._fragmentMappings[currentIndex];
                var fraction = (prog - mapping.Fragment.Start.TotalMilliseconds) /
                               mapping.Fragment.Duration.TotalMilliseconds;
                fraction = Math.Clamp(fraction, 0d, 1d);
                var (line, inlineLeft) = mapping.ComputeFraction(fraction);
                for (var i = 0; i < control._lineMappings.Count; i++)
                {
                    var lm = control._lineMappings[i];
                    var gradX = inlineLeft / lm.Width;
                    var delta = gradX == 0d ? 0d : 16f / lm.Width;
                    var startOffset = (i - line) switch
                    {
                        < 0 => 1f,
                        0 => (float)Math.Clamp(gradX, 0d, 1d),
                        > 0 => 0f
                    };
                    var endOffset = (i - line) switch
                    {
                        < 0 => 1f,
                        0 => (float)Math.Clamp(gradX + delta, 0d, 1d),
                        > 0 => 0f
                    };
                    var startAnimation = lm.GradientStart.Compositor.CreateScalarKeyFrameAnimation();
                    startAnimation.Duration = TimeSpan.FromMilliseconds(200);
                    startAnimation.InsertKeyFrame(1f, startOffset);
                    var endAnimation = lm.GradientEnd.Compositor.CreateScalarKeyFrameAnimation();
                    endAnimation.Duration = TimeSpan.FromMilliseconds(200);
                    endAnimation.InsertKeyFrame(1f, endOffset);
                    lm.GradientStart.StartAnimation(nameof(lm.GradientStart.Offset), startAnimation);
                    lm.GradientEnd.StartAnimation(nameof(lm.GradientEnd.Offset), endAnimation);
                }
            }
        }
        catch
        {
            // Ignored.
        }
        finally
        {
            control._viewSemaphore.Release();
        }
    }

    private void BuildFragmentMappings(LyricLine data)
    {
        if (data.B2BLyrics is not { Count: > 0 }) return;
        StringBuilder sb = new();
        foreach (var fragment in data.B2BLyrics)
        {
            var content = fragment.Text + "\uFEFF";
            _fragmentMappings.Add(new(fragment, content.Length));
            sb.Append(content);
        }
        _lineFullString =  sb.ToString();
    }

    private static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not LyricLineControl control ||
            e.NewValue is not double fontSize) return;
        control.FallbackLineTextBlock.FontSize = fontSize;
        control.TranslationTextBlock.FontSize = fontSize * 0.7;
        control.RebuildTextLayout();
    }
    
    private static void OnIsTranslationEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not LyricLineControl control ||
            e.NewValue is not bool isTranslationEnabled)
            return;
        control.TranslationTextBlock.Visibility = isTranslationEnabled && control._translatedLine.Length > 0
            ? Visibility.Visible
            : Visibility.Collapsed;
    }
    
    private static async void OnIsScaledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not LyricLineControl control ||
            e.NewValue is not bool isScaled)
            return;
        await control._viewSemaphore.WaitAsync();
        try
        {
            var visual = ElementCompositionPreview.GetElementVisual(control);
            var scaleAnimation = visual.Compositor.CreateSpringVector3Animation();
            scaleAnimation.FinalValue = new(isScaled ? (float)GlidyScroller.GetScaleFactor(d) : 1f);
            scaleAnimation.Period = TimeSpan.FromMilliseconds(150);
            visual.StartAnimation(nameof(visual.Scale), scaleAnimation);

            foreach (var lm in control._lineMappings)
            {
                lm.GradientStart.Offset = 0f;
                lm.GradientEnd.Offset = 0f;
            }

            Storyboard sb = new();
            DoubleAnimation opacityAnimation = new()
            {
                Duration = TimeSpan.FromMilliseconds(200),
                To = isScaled ? 1d : 0.6d
            };
            Storyboard.SetTarget(opacityAnimation, control.FallbackLineTextBlock);
            Storyboard.SetTargetProperty(opacityAnimation, nameof(control.FallbackLineTextBlock.Opacity));
            sb.Children.Add(opacityAnimation);
            sb.Begin();
        }
        finally
        {
            control._viewSemaphore.Release();
        }
    }
    
    private static void OnIsPreviewedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not LyricLineControl control ||
            e.NewValue is not bool previewed)
            return;
        control.UpdateClarity(previewed ? 1d : control.StaticClarity);
    }
    
    private static void OnStaticClarityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not LyricLineControl control ||
            e.NewValue is not double clarity)
            return;
        control.UpdateClarity(control.IsPreviewed ? 1d : clarity);
    }

    private void UpdateClarity(double clarity)
    {
        var visual = ElementCompositionPreview.GetElementVisual(this);
        var opacityAnimation = visual.Compositor.CreateSpringScalarAnimation();
        opacityAnimation.FinalValue = (float)clarity;
        opacityAnimation.Period = TimeSpan.FromMilliseconds(200);
        visual.StartAnimation(nameof(Visual.Opacity), opacityAnimation);
    }

    protected override void OnTapped(TappedRoutedEventArgs e)
    {
        _playbackServiceBridge.CurrentPositionMilliseconds = _start.TotalMilliseconds;
        e.Handled = true;
        base.OnTapped(e);
    }

    protected override void OnPointerEntered(PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerDeviceType is not PointerDeviceType.Touch)
            _backgroundVisual.Opacity = 0.4f;
        e.Handled = true;
        base.OnPointerEntered(e);
    }

    protected override void OnPointerPressed(PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerDeviceType is not PointerDeviceType.Touch)
            _backgroundVisual.Opacity = 0.8f;
        e.Handled = true;
        base.OnPointerEntered(e);
    }

    protected override void OnPointerReleased(PointerRoutedEventArgs e)
    {
        _backgroundVisual.Opacity = e.Pointer.PointerDeviceType is PointerDeviceType.Touch ? 0f : 0.4f;
        e.Handled = true;
        base.OnPointerReleased(e);
    }

    protected override void OnPointerExited(PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerDeviceType is not PointerDeviceType.Touch)
            _backgroundVisual.Opacity = 0f;
        e.Handled = true;
        base.OnPointerEntered(e);
    }

    protected override void OnPointerCaptureLost(PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerDeviceType is not PointerDeviceType.Touch)
        {
            _backgroundVisual.Opacity = 0f;
        }
        e.Handled = true;
        base.OnPointerCaptureLost(e);
    }

    [RelayCommand]
    private void OnB2BSizeChanged(SizeChangedEventArgs args)
    {
        if (args.NewSize.Width <= 0) return;
        //if (Math.Abs(args.NewSize.Width - args.PreviousSize.Width) == 0d) return;
        RebuildTextLayout();
    }

    private void RebuildTextLayout()
    {
        if (_lineFullString.Length is 0 ||
            B2BBackTextBlock.ActualWidth <= 0)
            return;
        
        using CanvasTextFormat format = new();
        format.FontFamily = new(_preference.Appearance.DisplayFont);
        format.FontSize = (float)BaseFontSize;
        format.FontWeight = FontWeights.Bold;
        format.WordWrapping = CanvasWordWrapping.Wrap;
        using CanvasTextLayout layout = new(
            CanvasDevice.GetSharedDevice(),
            _lineFullString,
            format,
            (float)this.ActualWidth,
            0
        );
        var pos = 0;
        var mappingIdx = 0;
        B2BHiddenPanel.Children.Clear();
        B2BHighlightPanel.Children.Clear();
        _lineMappings.Clear();
        int lineIdx = 0;
        var lastLeft = 0d;
        while (pos < _lineFullString.Length)
        {
            if (mappingIdx >= _fragmentMappings.Count) break;
            var mapping = _fragmentMappings[mappingIdx];
            mapping.Regions.Clear();
            foreach (var region in layout.GetCharacterRegions(pos, mapping.TextLength))
            {
                if (region.LayoutBounds.Left < lastLeft) lineIdx++;
                mapping.Regions.Add(new(lineIdx, region.LayoutBounds.Left, region.LayoutBounds.Width));
                lastLeft = region.LayoutBounds.Left;
            }
            pos += mapping.TextLength;
            mappingIdx++;
        }
        pos = 0;
        foreach (var lineMetric in layout.LineMetrics)
        {
            var lineText = _lineFullString.Substring(pos, lineMetric.CharacterCount);
            TextBlock hiddenLineTextBlock = new()
            {
                Text = lineText,
                FontWeight = FontWeights.Bold,
                FontSize = BaseFontSize,
                HorizontalAlignment = HorizontalAlignment.Left,
                TextAlignment = TextAlignment.Right
            };
            TextBlock highlightBorder = new()
            {
                FontSize = BaseFontSize,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            B2BHiddenPanel.Children.Add(hiddenLineTextBlock);
            B2BHighlightPanel.Children.Add(highlightBorder);
            pos += lineMetric.CharacterCount;
            lineIdx++;
        }
        for (var i = 0; i < layout.LineMetrics.Length; i++)
        {
            if (B2BHiddenPanel.Children[i] is not TextBlock hiddenLineTextBlock) continue;
            var lineVisual = ElementCompositionPreview.GetElementVisual(hiddenLineTextBlock);
            lineVisual.Opacity = 0;
            var gradient = lineVisual.Compositor.CreateLinearGradientBrush();
            var progressStartStop = lineVisual.Compositor.CreateColorGradientStop(0f, Colors.White);
            var progressEndStop = lineVisual.Compositor.CreateColorGradientStop(0f, Colors.Transparent);
            gradient.ColorStops.Add(progressStartStop);
            gradient.ColorStops.Add(progressEndStop);

            var mask = hiddenLineTextBlock.GetAlphaMask();
            var maskBrush = lineVisual.Compositor.CreateMaskBrush();
            maskBrush.Mask = mask;
            maskBrush.Source = gradient;
            
            var newVisual = lineVisual.Compositor.CreateSpriteVisual();
            newVisual.Brush = maskBrush;
            
            if (B2BHighlightPanel.Children[i] is not TextBlock highlightLineTextBlock) continue;
            ElementCompositionPreview.SetElementChildVisual(highlightLineTextBlock, newVisual);
            
            LineMapping lineMapping = new(progressStartStop, progressEndStop);
            _lineMappings.Add(lineMapping);

            hiddenLineTextBlock.AttachEventTriggerBehaviorToCommand<FrameworkElement, SizeChangedEventTriggerBehavior>
                (new RelayCommand<SizeChangedEventArgs>(e =>
                {
                    if (e is null) return;
                    newVisual.Size = new((float)e.NewSize.Width, (float)e.NewSize.Height);
                    lineMapping.Width = e.NewSize.Width;
                }));
        }
    }
}