using Cirrus.Extensions;
using Microsoft.UI;
using Microsoft.UI.Composition;

namespace Cirrus.Composition;

public class BorderComposition
{
    public Visual Visual => _containerVisual;

    private readonly Compositor _compositor;
    private readonly CompositionRoundedRectangleGeometry _outerGeometry, _innerGeometry, _containerGeometry;
    private readonly List<CompositionSpriteShape> _outerShapes;
    public List<SpriteVisual> InnerVisuals { get; }
    public List<ShapeVisual> OuterVisuals { get; }
    private readonly ContainerVisual _containerVisual;

    public BorderComposition(
        Compositor compositor,
        float width, float height,
        float borderThickness, float cornerRadius,
        CompositionBrush[] backgroundBrushes, CompositionBrush[] strokeBrushes
    )
    {
        _compositor = compositor;

        OuterVisuals = new();
        _outerShapes = new();
        var outerVisual = compositor.BuildContainerVisual(
            relativeSizeAdjustment: new(1f, 1f)
        );
        _outerGeometry = compositor.BuildRoundedRectangleGeometry(
            offset: new(borderThickness, borderThickness),
            size: new(width, height),
            cornerRadius: new(cornerRadius, cornerRadius)
        );
        foreach (var strokeBrush in strokeBrushes)
        {
            outerVisual.Children.InsertAtTop(compositor.BuildShapeVisual(
                out var visual,
                shapes:
                [
                    compositor.BuildSpriteShape(
                        out var shape,
                        geometry: _outerGeometry,
                        fillBrush: compositor.CreateColorBrush(Colors.Transparent),
                        strokeBrush: strokeBrush,
                        strokeThickness: borderThickness
                    )
                ],
                relativeSizeAdjustment: new(1f, 1f)
            ));
            _outerShapes.Add(shape);
            OuterVisuals.Add(visual);
        }

        InnerVisuals = new();
        var innerVisual = compositor.BuildContainerVisual(
            relativeSizeAdjustment: new(1f, 1f)
        );
        var innerClip = compositor.CreateGeometricClip(
            compositor.BuildRoundedRectangleGeometry(
                out _innerGeometry,
                offset: new(borderThickness, borderThickness),
                size: new(width, height),
                cornerRadius: new(cornerRadius - borderThickness, cornerRadius - borderThickness)
            )
        );
        foreach (var backgroundBrush in backgroundBrushes)
        {
            innerVisual.Children.InsertAtTop(compositor.BuildSpriteVisual(
                out var visual,
                relativeSizeAdjustment: new(1f, 1f),
                brush: backgroundBrush,
                clip: innerClip,
                opacity: 0f
            ));
            InnerVisuals.Add(visual);
        }

        _containerVisual = compositor.BuildContainerVisual(
            children:
            [
                innerVisual,
                outerVisual
            ],
            offset: new(-borderThickness, -borderThickness, 0f),
            size: new(width + 2 * borderThickness, height + 2 * borderThickness),
            clip: compositor.CreateGeometricClip(
                compositor.BuildRoundedRectangleGeometry(
                    out _containerGeometry,
                    offset: new(0f, 0f),
                    size: new(width + 2 * borderThickness, height + 2 * borderThickness),
                    cornerRadius: new(cornerRadius, cornerRadius)
                )
            )
        );
    }

    public void AnimateDimensions(
        float x, float y,
        float width, float height,
        float borderThickness, float cornerRadius,
        TimeSpan duration, TimeSpan delay
    )
    {
        _outerGeometry.StartAnimationGroup(_compositor.BuildAnimationGroupWith(
            _compositor.BuildVector2KeyFrameAnimation(
                finalValue: new(borderThickness, borderThickness),
                duration: duration,
                delay: delay,
                target: nameof(_outerGeometry.Offset)
            ),
            _compositor.BuildVector2KeyFrameAnimation(
                finalValue: new(width, height),
                duration: duration,
                delay: delay,
                target: nameof(_outerGeometry.Size)
            ),
            _compositor.BuildVector2KeyFrameAnimation(
                finalValue: new(cornerRadius, cornerRadius),
                duration: duration,
                delay: delay,
                target: nameof(_outerGeometry.CornerRadius)
            )
        ));
        _outerShapes.ForEach(os => os.StartAnimationGroup(
            _compositor.BuildScalarKeyFrameAnimation(
                finalValue: borderThickness,
                duration: duration,
                delay: delay,
                target: nameof(os.StrokeThickness)
            )
        ));
        _innerGeometry.StartAnimationGroup(_compositor.BuildAnimationGroupWith(
            _compositor.BuildVector2KeyFrameAnimation(
                finalValue: new(borderThickness, borderThickness),
                duration: duration,
                delay: delay,
                target: nameof(_innerGeometry.Offset)
            ),
            _compositor.BuildVector2KeyFrameAnimation(
                finalValue: new(width, height),
                duration: duration,
                delay: delay,
                target: nameof(_innerGeometry.Size)
            ),
            _compositor.BuildVector2KeyFrameAnimation(
                finalValue: new(cornerRadius - borderThickness, cornerRadius - borderThickness),
                duration: duration,
                delay: delay,
                target: nameof(_innerGeometry.CornerRadius)
            )
        ));
        _containerVisual.StartAnimationGroup(_compositor.BuildAnimationGroupWith(
            _compositor.BuildVector3KeyFrameAnimation(
                finalValue: new(x - borderThickness, y - borderThickness, 0),
                duration: duration,
                delay: delay,
                target: nameof(_containerVisual.Offset)
            ),
            _compositor.BuildVector2KeyFrameAnimation(
                finalValue: new(width + 2 * borderThickness, height + 2 * borderThickness),
                duration: duration,
                delay: delay,
                target: nameof(_containerVisual.Size)
            )
        ));
        _containerGeometry.StartAnimationGroup(_compositor.BuildAnimationGroupWith(
            _compositor.BuildVector2KeyFrameAnimation(
                finalValue: new(0f, 0f),
                duration: duration,
                delay: delay,
                target: nameof(_containerGeometry.Offset)
            ),
            _compositor.BuildVector2KeyFrameAnimation(
                finalValue: new(width + 2 * borderThickness, height + 2 * borderThickness),
                duration: duration,
                delay: delay,
                target: nameof(_containerGeometry.Size)
            ),
            _compositor.BuildVector2KeyFrameAnimation(
                finalValue: new(cornerRadius, cornerRadius),
                duration: duration,
                delay: delay,
                target: nameof(_containerGeometry
                .CornerRadius)
            )
        ));
    }
    
    public void UpdateSingleBackgroundBrush(CompositionBrush newBackgroundBrush)
    {
        if (InnerVisuals.SingleOrDefault() is not { } innerVisual) return;
        innerVisual.Brush = newBackgroundBrush;
    }

    public void UpdateSingleStrokeBrush(CompositionBrush newStrokeBrush)
    {
        if (_outerShapes.SingleOrDefault() is not { } outerShape) return;
        outerShape.StrokeBrush = newStrokeBrush;
    }
}