using Cirrus.Extensions;
using Cirrus.Primitives;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;

namespace Cirrus.Composition;

public class DualStateBorderComposition
{
    public record Dimensions(
        float X, float Y,
        float Width, float Height,
        float BorderThickness, float CornerRadius
    );

    private record BrushOpacity(
        float InitialOpacity,
        float CompactOpacity,
        float ExpandedOpacity
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
    private readonly BrushOpacity[] _backgroundOpacities, _borderOpacities;
    private readonly BorderComposition _borderComposition;
    private bool _isCompact;

    public Action<ElementTheme> ForceApplyTheme { get; }

    private DualStateBorderComposition(
        Compositor compositor,
        (CompositionBrush Brush, BrushOpacity OpacityConfig)[] backgroundBrushes,
        (CompositionBrush Brush, BrushOpacity OpacityConfig)[] borderBrushes,
        Action<ElementTheme> forceApplyThemeAction
    )
    {
        _compositor = compositor;
        _backgroundOpacities = backgroundBrushes.Select(b => b.OpacityConfig).ToArray();
        _borderOpacities = borderBrushes.Select(b => b.OpacityConfig).ToArray();
        _borderComposition = new(compositor, 0f, 0f, 0f, 0f,
            backgroundBrushes.Select(b => b.Brush).ToArray(),
            borderBrushes.Select(b => b.Brush).ToArray()
        );
        for (int i = 0; i < backgroundBrushes.Length; i++)
        {
            _borderComposition.InnerVisuals[i].Opacity = backgroundBrushes[i].OpacityConfig.InitialOpacity;
        }
        for (int i = 0; i < borderBrushes.Length; i++)
        {
            _borderComposition.OuterVisuals[i].Opacity = borderBrushes[i].OpacityConfig.InitialOpacity;
        }
        CompactDimensions = new(0f, 0f, 0f, 0f, 0f, 0f);
        ExpandedDimensions = new(0f, 0f, 0f, 0f, 0f, 0f);
        ForceApplyTheme = forceApplyThemeAction;
    }

    private EventHandler<BrushChangedEventArgs> BackgroundBrushChangedHandlerFactory(int index) =>
        (_, args) =>
        {
            _borderComposition.InnerVisuals[index].Brush = args.NewBrush;
        };

    public static DualStateBorderComposition CreateAcrylicBar(Compositor compositor)
    {
        var backgroundComposition = compositor.BuildDefaultThemeAwareAcrylicBrush();
        var borderComposition = compositor.BuildThemeAwareCardStrokeBrush();
        DualStateBorderComposition result = new(
            compositor,
            [
                (backgroundComposition.Resource.Brush, new(1f, 1f, 1f))
            ],
            [
                (borderComposition.Resource, new(1f, 1f, 1f))
            ],
            theme =>
            {
                backgroundComposition.ApplyTheme(theme);
                borderComposition.ApplyTheme(theme);
            }
        );
        backgroundComposition.Resource.BrushChanged += result.BackgroundBrushChangedHandlerFactory(0);
        return result;
    }

    public static DualStateBorderComposition CreateHeaderBar(Compositor compositor)
    {
        var backgroundComposition = compositor.BuildDefaultThemeAwareAcrylicBrush();
        var borderComposition = compositor.BuildThemeAwareCardStrokeBrush();
        DualStateBorderComposition result = new(
            compositor,
            [
                (backgroundComposition.Resource.Brush, new(0f, 1f, 0f))
            ],
            [
                (borderComposition.Resource, new(0f, 1f, 0f))
            ],
            theme =>
            {
                backgroundComposition.ApplyTheme(theme);
                borderComposition.ApplyTheme(theme);
            }
        );
        backgroundComposition.Resource.BrushChanged += result.BackgroundBrushChangedHandlerFactory(0);
        return result;
    }

    public static DualStateBorderComposition CreateHeaderCard(Compositor compositor)
    {
        var acrylicBackgroundComposition = compositor.BuildDefaultThemeAwareAcrylicBrush();
        var solidBackgroundComposition = compositor.BuildThemeAwareCardBackgroundSolidBrush();
        var borderComposition = compositor.BuildThemeAwareCardStrokeBrush();
        DualStateBorderComposition result = new(
            compositor,
            [
                (solidBackgroundComposition.Resource, new(1f, 0f, 1f)),
                (acrylicBackgroundComposition.Resource.Brush, new(0f, 1f, 0f))
            ],
            [
                (borderComposition.Resource, new(1f, 1f, 1f))
            ],
            theme =>
            {
                solidBackgroundComposition.ApplyTheme(theme);
                acrylicBackgroundComposition.ApplyTheme(theme);
                borderComposition.ApplyTheme(theme);
            }
        );
        acrylicBackgroundComposition.Resource.BrushChanged += result.BackgroundBrushChangedHandlerFactory(1);
        return result;
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
        for (var i = 0; i < _borderComposition.InnerVisuals.Count; i++)
        {
            var opacityConfig = _backgroundOpacities[i];
            _borderComposition.InnerVisuals[i].StartAnimationGroup(
                _compositor.BuildScalarKeyFrameAnimation(
                    finalValue: isCompact ? opacityConfig.CompactOpacity : opacityConfig.ExpandedOpacity,
                    duration: TimeSpan.FromMilliseconds(200),
                    target: nameof(SpriteVisual.Opacity)
                )
            );
        }
        for (var i = 0; i < _borderComposition.OuterVisuals.Count; i++)
        {
            var opacityConfig = _borderOpacities[i];
            _borderComposition.OuterVisuals[i].StartAnimationGroup(
                _compositor.BuildScalarKeyFrameAnimation(
                    finalValue: isCompact ? opacityConfig.CompactOpacity : opacityConfig.ExpandedOpacity,
                    duration: TimeSpan.FromMilliseconds(200),
                    target: nameof(SpriteVisual.Opacity)
                )
            );
        }
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