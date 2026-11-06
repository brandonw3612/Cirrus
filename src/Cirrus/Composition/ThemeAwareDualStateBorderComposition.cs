using Cirrus.Extensions;
using Cirrus.Primitives;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;

namespace Cirrus.Composition;

public class ThemeAwareDualStateBorderComposition
{
    public record Dimensions(
        float X, float Y,
        float Width, float Height,
        float BorderThickness, float CornerRadius
    );

    public Dimensions CompactDimensions
    {
        get;
        set
        {
            field = value;
            if (_isCompact) UpdateDimensions(value);
        }
    }

    public Dimensions ExpandedDimensions
    {
        get;
        set
        {
            field = value;
            if (!_isCompact) UpdateDimensions(value);
        }
    }
    
    public Visual Visual => _borderComposition.Visual;

    private readonly Compositor _compositor;
    private readonly BorderComposition _borderComposition;
    private readonly ThemeAwareResource<CompositionColorBrush> _solidBackgroundComposition;
    private readonly ThemeAwareResource<AcrylicComposition> _acrylicBackgroundComposition;
    private readonly ThemeAwareResource<CompositionColorBrush> _borderStrokeBrush;
    private bool _isCompact, _enableSolidBackground;
    
    public ThemeAwareDualStateBorderComposition(
        Compositor compositor,
        bool enableSolidBackground = true
    )
    {
        _compositor = compositor;
        _solidBackgroundComposition = compositor.BuildThemeAwareCardBackgroundSolidBrush();
        _acrylicBackgroundComposition = compositor.BuildDefaultThemeAwareAcrylicBrush();
        // TODO: Resource disposal needed
        _acrylicBackgroundComposition.Resource.BrushChanged += OnAcrylicBrushChanged;
        _borderStrokeBrush = compositor.BuildThemeAwareCardStrokeBrush();
        _borderComposition = new(compositor, 0f, 0f, 0f, 0f,
            [
                _solidBackgroundComposition.Resource,
                _acrylicBackgroundComposition.Resource.Brush,
            ],
            [_borderStrokeBrush.Resource]
        );
        _borderComposition.InnerVisuals[enableSolidBackground ? 0 : 1].Opacity = 1f;
        CompactDimensions = new(0f, 0f, 0f, 0f, 0f, 0f);
        ExpandedDimensions = new(0f, 0f, 0f, 0f, 0f, 0f);
        _enableSolidBackground = enableSolidBackground;
    }

    private void OnAcrylicBrushChanged(object? sender, EventArgs e)
    {
        _borderComposition.InnerVisuals[1].Brush = _acrylicBackgroundComposition.Resource.Brush;
    }

    public void ForceApplyTheme(ElementTheme theme)
    {
        _solidBackgroundComposition.ApplyTheme(theme);
        _acrylicBackgroundComposition.ApplyTheme(theme);
        _borderStrokeBrush.ApplyTheme(theme);
    }
    
    public void ToggleCompact(bool isCompact)
    {
        if (_isCompact == isCompact) return;
        _isCompact = isCompact;
        var dimensions = isCompact ? CompactDimensions : ExpandedDimensions;
        _borderComposition.AnimateDimensions(
            dimensions.X, dimensions.Y,
            dimensions.Width, dimensions.Height,
            dimensions.BorderThickness, dimensions.CornerRadius,
            TimeSpan.FromMilliseconds(400), TimeSpan.Zero
        );
        if (!_enableSolidBackground) return;
        // Solid
        _borderComposition.InnerVisuals[0].StartAnimationGroup(
            _compositor.BuildScalarKeyFrameAnimation(
                finalValue: isCompact ? 0f : 1f,
                duration: TimeSpan.FromMilliseconds(200),
                target: nameof(SpriteVisual.Opacity)
            )
        );
        // Acrylic
        _borderComposition.InnerVisuals[1].StartAnimationGroup(
            _compositor.BuildScalarKeyFrameAnimation(
                finalValue: isCompact ? 1f : 0f,
                duration: TimeSpan.FromMilliseconds(200),
                target: nameof(SpriteVisual.Opacity)
            )
        );
    }
    
    private void UpdateDimensions(Dimensions dimensions)
    {
        _borderComposition.AnimateDimensions(
            dimensions.X, dimensions.Y,
            dimensions.Width, dimensions.Height,
            dimensions.BorderThickness, dimensions.CornerRadius,
            TimeSpan.FromMilliseconds(1), TimeSpan.Zero
        );
    }
}