using System.Numerics;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using Windows.Graphics.Effects;
using Windows.System.Power;
using Windows.UI;
using Windows.UI.ViewManagement;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Composition;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Dispatching;
using MDirectXPixelFormat = Microsoft.Graphics.DirectX.DirectXPixelFormat;
using MDirectXAlphaMode = Microsoft.Graphics.DirectX.DirectXAlphaMode;
using WDirectXPixelFormat = Windows.Graphics.DirectX.DirectXPixelFormat;

namespace Cirrus.Composition;

public sealed class AcrylicComposition : IDisposable
{
    private const float BlurRadius = 30.0f;
    private const float NoiseOpacity = 0.02f;
    private const int NoiseSize = 256;
    private static readonly TimeSpan SwitchDuration = TimeSpan.FromMilliseconds(167);

    private const string TintColorColor = "TintColor.Color";
    private const string LuminosityColorColor = "LuminosityColor.Color";
    private const string OpaqueFallbackColorColor = "OpaqueFallbackColor.Color";
    private const string FallbackColorColor = "FallbackColor.Color";
    private const string FadeInOutWeight = "FadeInOut.CrossFade";

    private static readonly ConditionalWeakTable<Compositor, Dictionary<(bool, bool), CompositionEffectFactory>>
        s_factoryCache = new();

    private static readonly ConditionalWeakTable<Compositor, ICompositionSurface> s_noiseCache = new();

    private readonly Compositor _compositor;
    private readonly DispatcherQueue _dispatcher;
    private readonly List<SpriteVisual> _targets = new();
    private readonly UISettings _uiSettings = new();
    private readonly AccessibilitySettings _a11y = new();

    private CompositionBrush _brush = null!;
    private readonly CompositionSurfaceBrush _noiseBrush;
    private CompositionBackdropBrush? _defaultBackdrop;
    private CompositionBrush? _backdropSource;

    private bool _isUsingAcrylicBrush;
    private bool _isUsingOpaqueBrush;
    private bool _isDisabledByPolicy;
    private bool _isWaitingForFallbackAnimationComplete;
    private CompositionScopedBatch? _fallbackBatch;
    private bool _disposed;

    private Color _tintColor = Color.FromArgb(0xFF, 0xFC, 0xFC, 0xFC);
    private double _tintOpacity = 0.8;
    private double? _tintLuminosityOpacity;
    private Color _fallbackColor = Color.FromArgb(0xFF, 0xF9, 0xF9, 0xF9);
    private bool _alwaysUseFallback;

    public AcrylicComposition(Compositor compositor)
    {
        _compositor = compositor;
        _dispatcher = compositor.DispatcherQueue;

        _noiseBrush = compositor.CreateSurfaceBrush(GetOrCreateNoiseSurface(compositor));
        _noiseBrush.Stretch = CompositionStretch.None;

        _uiSettings.AdvancedEffectsEnabledChanged += OnPolicyChanged;
        _uiSettings.ColorValuesChanged += OnPolicyChanged;
        PowerManager.EnergySaverStatusChanged += OnPolicyChanged;
        _isDisabledByPolicy = ComputeIsDisabledByPolicy();

        _isUsingAcrylicBrush = !(_isDisabledByPolicy || _alwaysUseFallback);
        CreateAcrylicBrush(useCrossFadeEffect: false);
    }

    /// <summary>The current brush. It is replaced when switching between acrylic and fallback, so prefer Attach() over holding a reference to it.</summary>
    public CompositionBrush Brush => _brush;

    /// <summary>Raised when the underlying CompositionBrush is replaced.</summary>
    public event EventHandler? BrushChanged;

    /// <summary>
    /// The content to blur. Defaults to null, which uses CreateBackdropBrush() (whatever sits behind the visual in the same visual tree).
    /// Can also be any brush, such as a surface brush made from a CompositionVisualSurface, to blur a specific visual.
    /// The caller owns this brush's lifetime.
    /// </summary>
    public CompositionBrush? BackdropSource
    {
        get => _backdropSource;
        set
        {
            _backdropSource = value;
            RecreateBrush();
        }
    }

    public Color TintColor
    {
        get => _tintColor;
        set
        {
            _tintColor = value;
            OnTintChanged(colorChanged: true);
        }
    }

    public double TintOpacity
    {
        get => _tintOpacity;
        set
        {
            _tintOpacity = Math.Clamp(value, 0, 1);
            OnTintChanged(colorChanged: true);
        }
    }

    /// <summary>When null, the luminosity opacity is derived using the official algorithm.</summary>
    public double? TintLuminosityOpacity
    {
        get => _tintLuminosityOpacity;
        set
        {
            _tintLuminosityOpacity = value is double d ? Math.Clamp(d, 0, 1) : null;
            OnTintChanged(colorChanged: false);
        }
    }

    public Color FallbackColor
    {
        get => _fallbackColor;
        set
        {
            _fallbackColor = value;
            OnFallbackColorChanged();
        }
    }

    public TimeSpan TintTransitionDuration { get; set; } = TimeSpan.FromMilliseconds(500);

    public bool AlwaysUseFallback
    {
        get => _alwaysUseFallback;
        set
        {
            _alwaysUseFallback = value;
            UpdateAcrylicBrush();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        CancelFallbackAnimationCompleteWait();
        _uiSettings.AdvancedEffectsEnabledChanged -= OnPolicyChanged;
        _uiSettings.ColorValuesChanged -= OnPolicyChanged;
        PowerManager.EnergySaverStatusChanged -= OnPolicyChanged;

        foreach (var v in _targets)
            if (v.Brush == _brush)
                v.Brush = null;
        _targets.Clear();

        _brush.Dispose();
        _noiseBrush.Dispose();
        _defaultBackdrop?.Dispose();
    }

    private bool ComputeIsDisabledByPolicy() =>
        !_uiSettings.AdvancedEffectsEnabled ||
        PowerManager.EnergySaverStatus == EnergySaverStatus.On ||
        IsHighContrast();

    private bool IsHighContrast()
    {
        try
        {
            return _a11y.HighContrast;
        }
        catch
        {
            return false;
        }
    }

    private void OnPolicyChanged(object? sender, object? args)
    {
        _dispatcher.TryEnqueue(() =>
        {
            if (_disposed) return;
            _isDisabledByPolicy = ComputeIsDisabledByPolicy();
            UpdateAcrylicBrush();
        });
    }

    private void OnTintChanged(bool colorChanged)
    {
        if (_disposed) return;
        bool shouldUseOpaqueBrush = GetEffectiveTintColor().A == 255;

        if (_isUsingOpaqueBrush != shouldUseOpaqueBrush)
            UpdateAcrylicBrush();

        if (_isUsingAcrylicBrush || _isWaitingForFallbackAnimationComplete)
        {
            if (colorChanged)
                _brush.StartAnimation(TintColorColor,
                    MakeColorAnimation(GetEffectiveTintColor(), TintTransitionDuration));

            if (!shouldUseOpaqueBrush)
                _brush.StartAnimation(LuminosityColorColor,
                    MakeColorAnimation(GetEffectiveLuminosityColor(), TintTransitionDuration));
        }
    }

    private void OnFallbackColorChanged()
    {
        if (_disposed) return;

        if (!_isUsingOpaqueBrush && (_isUsingAcrylicBrush || _isWaitingForFallbackAnimationComplete))
        {
            var opaque = _fallbackColor;
            opaque.A = 0xFF;
            _brush.Properties.InsertColor(OpaqueFallbackColorColor, opaque);
        }

        if (_isUsingAcrylicBrush && !_isWaitingForFallbackAnimationComplete) return;

        _brush.StartAnimation(
            (_isUsingAcrylicBrush || _isWaitingForFallbackAnimationComplete) ? FallbackColorColor : "Color",
            MakeColorAnimation(_fallbackColor, TintTransitionDuration));
    }

    private void RecreateBrush()
    {
        if (_disposed) return;
        CreateAcrylicBrush(useCrossFadeEffect: false);
    }

    private void UpdateAcrylicBrush()
    {
        if (_disposed) return;

        bool isUsingAcrylicBrush = !(_isDisabledByPolicy || _alwaysUseFallback);
        bool shouldUseOpaqueBrush = GetEffectiveTintColor().A == 255;

        if (_isUsingOpaqueBrush != shouldUseOpaqueBrush)
        {
            _isUsingAcrylicBrush = isUsingAcrylicBrush;
            CreateAcrylicBrush(useCrossFadeEffect: false);
            return;
        }

        if (_isUsingAcrylicBrush != isUsingAcrylicBrush)
        {
            CancelFallbackAnimationCompleteWait();
            CreateAcrylicBrush(useCrossFadeEffect: true, forceCreateAcrylicBrush: true);

            _isUsingAcrylicBrush = isUsingAcrylicBrush;
            _isWaitingForFallbackAnimationComplete = true;

            float start = _isUsingAcrylicBrush ? 0f : 1f;
            float end = _isUsingAcrylicBrush ? 1f : 0f;
            PlayCrossFadeAnimation(start, end);
        }
    }

    private void CreateAcrylicBrush(bool useCrossFadeEffect, bool forceCreateAcrylicBrush = false)
    {
        CancelFallbackAnimationCompleteWait();

        CompositionBrush newBrush;
        if (forceCreateAcrylicBrush || _isUsingAcrylicBrush)
        {
            var tintColor = GetEffectiveTintColor();
            var luminosityColor = GetEffectiveLuminosityColor();
            _isUsingOpaqueBrush = tintColor.A == 255;

            var factory = GetOrCreateEffectFactory(_compositor, _isUsingOpaqueBrush, useCrossFadeEffect);
            var brush = factory.CreateBrush();

            if (!_isUsingOpaqueBrush)
                brush.SetSourceParameter("Backdrop",
                    _backdropSource ?? (_defaultBackdrop ??= _compositor.CreateBackdropBrush()));

            brush.SetSourceParameter("Noise", _noiseBrush);
            brush.Properties.InsertColor(TintColorColor, tintColor);

            if (!_isUsingOpaqueBrush)
            {
                brush.Properties.InsertColor(LuminosityColorColor, luminosityColor);
                var opaqueFallback = _fallbackColor;
                opaqueFallback.A = 0xFF;
                brush.Properties.InsertColor(OpaqueFallbackColorColor, opaqueFallback);
            }

            if (useCrossFadeEffect)
                brush.Properties.InsertColor(FallbackColorColor, _fallbackColor);

            newBrush = brush;
        }
        else
        {
            newBrush = _compositor.CreateColorBrush(_fallbackColor);
        }

        SetBrush(newBrush);
    }

    private void SetBrush(CompositionBrush newBrush)
    {
        var old = _brush;
        _brush = newBrush;

        foreach (var v in _targets) v.Brush = newBrush;
        BrushChanged?.Invoke(this, EventArgs.Empty);

        old?.Dispose();
    }

    private static ICompositionSurface GetOrCreateNoiseSurface(Compositor compositor)
    {
        if (s_noiseCache.TryGetValue(compositor, out var cached)) return cached;

        var device = CanvasDevice.GetSharedDevice();
        var graphicsDevice = CanvasComposition.CreateCompositionGraphicsDevice(compositor, device);
        var surface = graphicsDevice.CreateDrawingSurface(
            new Size(NoiseSize, NoiseSize),
            MDirectXPixelFormat.B8G8R8A8UIntNormalized,
            MDirectXAlphaMode.Premultiplied);

        var pixels = new byte[NoiseSize * NoiseSize * 4];
        var rnd = new Random(0x5EED);
        for (int i = 0; i < pixels.Length; i += 4)
        {
            byte v = (byte)rnd.Next(256);
            pixels[i] = v;
            pixels[i + 1] = v;
            pixels[i + 2] = v;
            pixels[i + 3] = 0xFF;
        }

        using (var bitmap = CanvasBitmap.CreateFromBytes(device, pixels, NoiseSize, NoiseSize,
                   WDirectXPixelFormat.B8G8R8A8UIntNormalized))
        using (var ds = CanvasComposition.CreateDrawingSession(surface))
        {
            ds.Clear(Colors.Transparent);
            ds.DrawImage(bitmap);
        }

        s_noiseCache.AddOrUpdate(compositor, surface);
        return surface;
    }

    private static CompositionEffectFactory GetOrCreateEffectFactory(Compositor compositor, bool opaque, bool crossFade)
    {
        var cache = s_factoryCache.GetOrCreateValue(compositor);
        if (!cache.TryGetValue((opaque, crossFade), out var factory))
        {
            factory = CreateEffectFactory(compositor, opaque, crossFade);
            cache[(opaque, crossFade)] = factory;
        }

        return factory;
    }

    private static CompositionEffectFactory CreateEffectFactory(Compositor compositor, bool opaque, bool crossFade)
    {
        var animated = new List<string> { TintColorColor };
        var tintColorEffect = new ColorSourceEffect { Name = "TintColor" };
        IGraphicsEffect tintOutput;

        if (opaque)
        {
            tintOutput = tintColorEffect;
        }
        else
        {
            var opaqueFallback = new ColorSourceEffect { Name = "OpaqueFallbackColor" };
            animated.Add(OpaqueFallbackColorColor);

            var backdropOnFallback = new CompositeEffect { Mode = CanvasComposite.SourceOver };
            backdropOnFallback.Sources.Add(opaqueFallback);
            backdropOnFallback.Sources.Add(new CompositionEffectSourceParameter("Backdrop"));

            var blur = new GaussianBlurEffect
            {
                Name = "Blur",
                BorderMode = EffectBorderMode.Hard,
                BlurAmount = BlurRadius,
                Source = backdropOnFallback
            };

            var luminosityColor = new ColorSourceEffect { Name = "LuminosityColor" };
            animated.Add(LuminosityColorColor);

            // The official code notes that Composition has Color and Luminosity swapped; this follows the official code
            var luminosityBlend = new BlendEffect
                { Mode = BlendEffectMode.Color, Background = blur, Foreground = luminosityColor };
            var colorBlend = new BlendEffect
                { Mode = BlendEffectMode.Luminosity, Background = luminosityBlend, Foreground = tintColorEffect };
            tintOutput = colorBlend;
        }

        var noiseBorder = new BorderEffect
        {
            ExtendX = CanvasEdgeBehavior.Wrap,
            ExtendY = CanvasEdgeBehavior.Wrap,
            Source = new CompositionEffectSourceParameter("Noise")
        };
        var noiseOpacity = new OpacityEffect { Name = "NoiseOpacity", Opacity = NoiseOpacity, Source = noiseBorder };

        var withNoise = new CompositeEffect { Mode = CanvasComposite.SourceOver };
        withNoise.Sources.Add(tintOutput);
        withNoise.Sources.Add(noiseOpacity);

        if (!crossFade)
            return compositor.CreateEffectFactory(withNoise, animated);

        var fallbackColor = new ColorSourceEffect { Name = "FallbackColor" };
        var fade = new CrossFadeEffect
            { Name = "FadeInOut", Source1 = fallbackColor, Source2 = withNoise, CrossFade = 1.0f };
        animated.Add(FallbackColorColor);
        animated.Add(FadeInOutWeight);
        return compositor.CreateEffectFactory(fade, animated);
    }

    private void PlayCrossFadeAnimation(float acrylicStart, float acrylicEnd)
    {
        var batch = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

        var easing = _compositor.CreateCubicBezierEasingFunction(new Vector2(0.5f, 0f), new Vector2(0f, 0.9f));
        var anim = _compositor.CreateScalarKeyFrameAnimation();
        anim.InsertKeyFrame(0f, acrylicStart);
        anim.InsertKeyFrame(1f, acrylicEnd, easing);
        anim.Duration = _uiSettings.AnimationsEnabled ? SwitchDuration : TimeSpan.FromMilliseconds(1);
        _brush.StartAnimation(FadeInOutWeight, anim);

        batch.End();
        _fallbackBatch = batch;
        batch.Completed += OnFallbackAnimationCompleted;
    }

    private void OnFallbackAnimationCompleted(object sender, CompositionBatchCompletedEventArgs args)
    {
        if (_disposed || !ReferenceEquals(sender, _fallbackBatch)) return;
        CancelFallbackAnimationCompleteWait();
        CreateAcrylicBrush(useCrossFadeEffect: false);
    }

    private void CancelFallbackAnimationCompleteWait()
    {
        _isWaitingForFallbackAnimationComplete = false;
        if (_fallbackBatch != null)
        {
            _fallbackBatch.Completed -= OnFallbackAnimationCompleted;
            _fallbackBatch = null;
        }
    }

    private CompositionAnimation MakeColorAnimation(Color color, TimeSpan duration)
    {
        var anim = _compositor.CreateColorKeyFrameAnimation();
        anim.InsertKeyFrame(1f, color);
        anim.Duration = !_uiSettings.AnimationsEnabled || duration == TimeSpan.Zero
            ? TimeSpan.FromMilliseconds(1)
            : duration;
        return anim;
    }

    private Color GetEffectiveTintColor()
    {
        var tint = _tintColor;
        if (_tintLuminosityOpacity.HasValue)
            tint.A = (byte)Math.Round(tint.A * _tintOpacity);
        else
            tint.A = (byte)Math.Round(tint.A * _tintOpacity * GetTintOpacityModifier(tint));
        return tint;
    }

    private Color GetEffectiveLuminosityColor()
    {
        var tint = _tintColor;
        tint.A = (byte)Math.Round(tint.A * _tintOpacity);
        return GetLuminosityColor(tint, _tintLuminosityOpacity);
    }

    private static double GetTintOpacityModifier(Color tint)
    {
        const double midPoint = 0.50;
        const double whiteMaxOpacity = 0.45;
        const double midPointMaxOpacity = 0.90;
        const double blackMaxOpacity = 0.85;

        var (_, s, v) = RgbToHsv(tint);
        double modifier = midPointMaxOpacity;

        if (v != midPoint)
        {
            double lowestMaxOpacity;
            double maxDeviation = midPoint;

            if (v > midPoint)
            {
                lowestMaxOpacity = whiteMaxOpacity;
                maxDeviation = 1 - maxDeviation;
            }
            else
            {
                lowestMaxOpacity = blackMaxOpacity;
            }

            double maxSuppression = midPointMaxOpacity - lowestMaxOpacity;
            double normalizedDeviation = Math.Abs(v - midPoint) / maxDeviation;
            if (s > 0) maxSuppression *= Math.Max(1 - s * 2, 0.0);

            modifier = midPointMaxOpacity - maxSuppression * normalizedDeviation;
        }

        return modifier;
    }

    private static Color GetLuminosityColor(Color tint, double? luminosityOpacity)
    {
        if (luminosityOpacity.HasValue)
            return Color.FromArgb((byte)Math.Round(luminosityOpacity.Value * 255), tint.R, tint.G, tint.B);

        var (h, s, v) = RgbToHsv(tint);
        v = Math.Clamp(v, 0.125, 0.965);
        var (r, g, b) = HsvToRgb(h, s, v);

        const double minLum = 0.15, maxLum = 1.03;
        double lumOpacity = Math.Min(tint.A / 255.0 * (maxLum - minLum) + minLum, 1.0);

        return Color.FromArgb((byte)Math.Round(lumOpacity * 255),
            (byte)Math.Round(r * 255), (byte)Math.Round(g * 255), (byte)Math.Round(b * 255));
    }

    private static (double h, double s, double v) RgbToHsv(Color c)
    {
        double r = c.R / 255.0, g = c.G / 255.0, b = c.B / 255.0;
        double max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b)), d = max - min;
        double h = 0;
        if (d > 0)
        {
            if (max == r) h = 60 * (((g - b) / d) % 6);
            else if (max == g) h = 60 * ((b - r) / d + 2);
            else h = 60 * ((r - g) / d + 4);
        }

        if (h < 0) h += 360;
        return (h, max == 0 ? 0 : d / max, max);
    }

    private static (double r, double g, double b) HsvToRgb(double h, double s, double v)
    {
        double c = v * s, x = c * (1 - Math.Abs((h / 60) % 2 - 1)), m = v - c;
        var (r, g, b) = ((int)(h / 60) % 6) switch
        {
            0 => (c, x, 0d), 1 => (x, c, 0d), 2 => (0d, c, x),
            3 => (0d, x, c), 4 => (x, 0d, c), _ => (c, 0d, x),
        };
        return (r + m, g + m, b + m);
    }
}