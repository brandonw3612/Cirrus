using System.Numerics;
using Windows.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;

namespace Cirrus.Extensions;

public static class CompositionVisualExtensions
{
    public static void SetChildElementVisual(this UIElement element, Visual visual)
    {
        ElementCompositionPreview.SetElementChildVisual(element, visual);
    }
    
    public static SpriteVisual BuildSpriteVisual(this Compositor compositor,
        Vector2? size = null, Vector3? offset = null, Vector3? scale = null,
        Vector2? relativeSizeAdjustment = null, Vector3? relativeOffsetAdjustment = null,
        CompositionBrush? brush = null, Visual[]? children = null,
        CompositionShadow? shadow = null, CompositionClip? clip = null,
        bool? isVisible = null, float? opacity = null)
    {
        var visual = compositor.CreateSpriteVisual();
        if (size is not null) visual.Size = size.Value;
        if (offset is not null) visual.Offset = offset.Value;
        if (scale is not null) visual.Scale = scale.Value;
        if (relativeSizeAdjustment is not null) visual.RelativeSizeAdjustment = relativeSizeAdjustment.Value;
        if (relativeOffsetAdjustment is not null) visual.RelativeOffsetAdjustment = relativeOffsetAdjustment.Value;
        if (brush is not null) visual.Brush = brush;
        if (children is not null)
        {
            foreach (var child in children)
            {
                visual.Children.InsertAtTop(child);
            }
        }
        if (shadow is not null) visual.Shadow = shadow;
        if (clip is not null) visual.Clip = clip;
        if (isVisible is not null) visual.IsVisible = isVisible.Value;
        if (opacity is not null) visual.Opacity = opacity.Value;
        return visual;
    }

    public static SpriteVisual BuildSpriteVisual(this Compositor compositor, out SpriteVisual visual,
        Vector2? size = null, Vector3? offset = null, Vector3? scale = null,
        Vector2? relativeSizeAdjustment = null, Vector3? relativeOffsetAdjustment = null,
        CompositionBrush? brush = null, Visual[]? children = null,
        CompositionShadow? shadow = null, CompositionClip? clip = null,
        bool? isVisible = null, float? opacity = null)
    {
        visual = compositor.BuildSpriteVisual(size, offset, scale, relativeSizeAdjustment, relativeOffsetAdjustment,
            brush, children, shadow, clip, isVisible, opacity);
        return visual;
    }
    
    public static CompositionColorBrush BuildColorBrush(this Compositor compositor, out CompositionColorBrush brush,
        Color color)
    {
        brush = compositor.CreateColorBrush(color);
        return brush;
    }
    
    public static CompositionRoundedRectangleGeometry BuildRoundedRectangleGeometry(this Compositor compositor,
        Vector2? cornerRadius = null, Vector2? size = null)
    {
        var geometry = compositor.CreateRoundedRectangleGeometry();
        if (cornerRadius is not null) geometry.CornerRadius = cornerRadius.Value;
        if (size is not null) geometry.Size = size.Value;
        return geometry;
    }
    
    public static CompositionRoundedRectangleGeometry BuildRoundedRectangleGeometry(this Compositor compositor,
        out CompositionRoundedRectangleGeometry geometry, Vector2? cornerRadius = null, Vector2? size = null)
    {
        geometry = compositor.BuildRoundedRectangleGeometry(cornerRadius, size);
        return geometry;
    }
    
    public static CompositionGeometricClip BuildGeometricClip(this Compositor compositor, out CompositionGeometricClip clip,
        CompositionGeometry geometry)
    {
        clip = compositor.CreateGeometricClip(geometry);
        return clip;
    }
    
    public static ShapeVisual BuildShapeVisual(this Compositor compositor,
        Vector2? size = null, Vector3? offset = null, Vector3? scale = null,
        Vector2? relativeSizeAdjustment = null, Vector3? relativeOffsetAdjustment = null,
        CompositionShape[]? shapes = null, CompositionClip? clip = null,
        bool? isVisible = null, float? opacity = null)
    {
        var visual = compositor.CreateShapeVisual();
        if (size is not null) visual.Size = size.Value;
        if (offset is not null) visual.Offset = offset.Value;
        if (scale is not null) visual.Scale = scale.Value;
        if (relativeSizeAdjustment is not null) visual.RelativeSizeAdjustment = relativeSizeAdjustment.Value;
        if (relativeOffsetAdjustment is not null) visual.RelativeOffsetAdjustment = relativeOffsetAdjustment.Value;
        if (shapes is not null)
        {
            foreach (var shape in shapes)
            {
                visual.Shapes.Add(shape);
            }
        }
        if (clip is not null) visual.Clip = clip;
        if (isVisible is not null) visual.IsVisible = isVisible.Value;
        if (opacity is not null) visual.Opacity = opacity.Value;
        return visual;
    }
    
    public static ShapeVisual BuildShapeVisual(this Compositor compositor, out ShapeVisual visual,
        Vector2? size = null, Vector3? offset = null, Vector3? scale = null,
        Vector2? relativeSizeAdjustment = null, Vector3? relativeOffsetAdjustment = null,
        CompositionShape[]? shapes = null, CompositionClip? clip = null,
        bool? isVisible = null, float? opacity = null)
    {
        visual = compositor.BuildShapeVisual(size, offset, scale, relativeSizeAdjustment, relativeOffsetAdjustment,
            shapes, clip, isVisible, opacity);
        return visual;
    }
    
    public static CompositionSpriteShape BuildSpriteShape(this Compositor compositor,
        CompositionGeometry? geometry = null,
        Vector2? offset = null, Vector2? scale = null,
        CompositionBrush? fillBrush = null, CompositionBrush? strokeBrush = null,
        float? strokeThickness = null, float[]? strokeDashArray = null, CompositionStrokeCap? strokeDashCap = null,
        float? strokeDashOffset = null, CompositionStrokeCap? strokeStartCap = null, CompositionStrokeCap? strokeEndCap = null,
        float? strokeMiterLimit = null, CompositionStrokeLineJoin? strokeLineJoin = null,
        bool? isStrokeNonScaling = null)
    {
        var shape = geometry is null ? compositor.CreateSpriteShape() : compositor.CreateSpriteShape(geometry);
        if (offset is not null) shape.Offset = offset.Value;
        if (scale is not null) shape.Scale = scale.Value;
        if (fillBrush is not null) shape.FillBrush = fillBrush;
        if (strokeBrush is not null) shape.StrokeBrush = strokeBrush;
        if (strokeThickness is not null) shape.StrokeThickness = strokeThickness.Value;
        if (strokeDashArray is not null)
        {
            foreach (var dash in strokeDashArray)
            {
                shape.StrokeDashArray.Add(dash);
            }
        }
        if (strokeDashCap is not null) shape.StrokeDashCap = strokeDashCap.Value;
        if (strokeDashOffset is not null) shape.StrokeDashOffset = strokeDashOffset.Value;
        if (strokeStartCap is not null) shape.StrokeStartCap = strokeStartCap.Value;
        if (strokeEndCap is not null) shape.StrokeEndCap = strokeEndCap.Value;
        if (strokeMiterLimit is not null) shape.StrokeMiterLimit = strokeMiterLimit.Value;
        if (strokeLineJoin is not null) shape.StrokeLineJoin = strokeLineJoin.Value;
        if (isStrokeNonScaling is not null) shape.IsStrokeNonScaling = isStrokeNonScaling.Value;
        return shape;
    }
    
    public static CompositionSpriteShape BuildSpriteShape(this Compositor compositor, out CompositionSpriteShape shape,
        CompositionGeometry? geometry = null,
        Vector2? offset = null, Vector2? scale = null,
        CompositionBrush? fillBrush = null, CompositionBrush? strokeBrush = null,
        float? strokeThickness = null, float[]? strokeDashArray = null, CompositionStrokeCap? strokeDashCap = null,
        float? strokeDashOffset = null, CompositionStrokeCap? strokeStartCap = null, CompositionStrokeCap? strokeEndCap = null,
        float? strokeMiterLimit = null, CompositionStrokeLineJoin? strokeLineJoin = null,
        bool? isStrokeNonScaling = null)
    {
        shape = compositor.BuildSpriteShape(geometry, offset, scale, fillBrush, strokeBrush, strokeThickness,
            strokeDashArray, strokeDashCap, strokeDashOffset, strokeStartCap, strokeEndCap, strokeMiterLimit,
            strokeLineJoin, isStrokeNonScaling);
        return shape;
    }
}