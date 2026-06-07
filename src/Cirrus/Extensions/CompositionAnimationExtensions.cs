using System.Numerics;
using Microsoft.UI.Composition;

namespace Cirrus.Extensions;

public static class CompositionAnimationExtensions
{
    public static CompositionAnimationGroup BuildAnimationGroupWith
        (this Compositor compositor, params CompositionAnimation[] animations)
    {
        var animationGroup = compositor.CreateAnimationGroup();
        foreach (var animation in animations)
        {
            animationGroup.Add(animation);
        }
        return animationGroup;
    }
    
    public static BooleanKeyFrameAnimation BuildBooleanKeyFrameAnimation
        (this Compositor compositor, bool? initialValue = null, bool? finalValue = null, 
            TimeSpan? duration = null, TimeSpan? delay = null, string? target = null)
    {
        var animation = compositor.CreateBooleanKeyFrameAnimation();
        if (initialValue is not null) animation.InsertKeyFrame(0f, initialValue.Value);
        if (finalValue is not null) animation.InsertKeyFrame(1f, finalValue.Value);
        if (duration is not null) animation.Duration = duration.Value;
        if (delay is not null) animation.DelayTime = delay.Value;
        if (target is not null) animation.Target = target;
        return animation;
    }
    
    public static ScalarKeyFrameAnimation BuildScalarKeyFrameAnimation
        (this Compositor compositor, float? initialValue = null, float? finalValue = null,
            TimeSpan? duration = null, TimeSpan? delay = null, string? target = null)
    {
        var animation = compositor.CreateScalarKeyFrameAnimation();
        if (initialValue is not null) animation.InsertKeyFrame(0f, initialValue.Value);
        if (finalValue is not null) animation.InsertKeyFrame(1f, finalValue.Value);
        if (duration is not null) animation.Duration = duration.Value;
        if (delay is not null) animation.DelayTime = delay.Value;
        if (target is not null) animation.Target = target;
        return animation;
    }
    
    public static Vector2KeyFrameAnimation BuildVector2KeyFrameAnimation
        (this Compositor compositor, Vector2? initialValue = null, Vector2? finalValue = null,
            TimeSpan? duration = null, TimeSpan? delay = null, string? target = null)
    {
        var animation = compositor.CreateVector2KeyFrameAnimation();
        if (initialValue is not null) animation.InsertKeyFrame(0f, initialValue.Value);
        if (finalValue is not null) animation.InsertKeyFrame(1f, finalValue.Value);
        if (duration is not null) animation.Duration = duration.Value;
        if (delay is not null) animation.DelayTime = delay.Value;
        if (target is not null) animation.Target = target;
        return animation;
    }
    
    public static Vector3KeyFrameAnimation BuildVector3KeyFrameAnimation
        (this Compositor compositor, Vector3? initialValue = null, Vector3? finalValue = null,
            TimeSpan? duration = null, TimeSpan? delay = null, string? target = null)
    {
        var animation = compositor.CreateVector3KeyFrameAnimation();
        if (initialValue is not null) animation.InsertKeyFrame(0f, initialValue.Value);
        if (finalValue is not null) animation.InsertKeyFrame(1f, finalValue.Value);
        if (duration is not null) animation.Duration = duration.Value;
        if (delay is not null) animation.DelayTime = delay.Value;
        if (target is not null) animation.Target = target;
        return animation;
    }

    public static SpringScalarNaturalMotionAnimation BuildSpringScalarAnimation
        (this Compositor compositor, float? initialValue = null, float? finalValue = null, 
            TimeSpan? period = null, float? dampingRatio = null, string? target = null)
    {
        var animation = compositor.CreateSpringScalarAnimation();
        if (initialValue is not null) animation.InitialValue = initialValue.Value;
        if (finalValue is not null) animation.FinalValue = finalValue.Value;
        if (dampingRatio is not null) animation.DampingRatio = dampingRatio.Value;
        if (period is not null) animation.Period = period.Value;
        if (target is not null) animation.Target = target;
        return animation;
    }
    
    public static SpringVector2NaturalMotionAnimation BuildSpringVector2Animation
        (this Compositor compositor, Vector2? initialValue = null, Vector2? finalValue = null, 
            TimeSpan? period = null, float? dampingRatio = null, string? target = null)
    {
        var animation = compositor.CreateSpringVector2Animation();
        if (initialValue is not null) animation.InitialValue = initialValue.Value;
        if (finalValue is not null) animation.FinalValue = finalValue.Value;
        if (dampingRatio is not null) animation.DampingRatio = dampingRatio.Value;
        if (period is not null) animation.Period = period.Value;
        if (target is not null) animation.Target = target;
        return animation;
    }
    
    public static SpringVector3NaturalMotionAnimation BuildSpringVector3Animation
        (this Compositor compositor, Vector3? initialValue = null, Vector3? finalValue = null, 
            TimeSpan? period = null, float? dampingRatio = null, string? target = null)
    {
        var animation = compositor.CreateSpringVector3Animation();
        if (initialValue is not null) animation.InitialValue = initialValue.Value;
        if (finalValue is not null) animation.FinalValue = finalValue.Value;
        if (dampingRatio is not null) animation.DampingRatio = dampingRatio.Value;
        if (period is not null) animation.Period = period.Value;
        if (target is not null) animation.Target = target;
        return animation;
    }

    public static BooleanKeyFrameAnimation WithKeyFrames(this BooleanKeyFrameAnimation animation,
        params (float frameKey, bool frameValue)[] frames)
    {
        foreach (var (frameKey, frameValue) in frames)
        {
            animation.InsertKeyFrame(frameKey, frameValue);
        }
        return animation;
    }
    
    public static ScalarKeyFrameAnimation WithKeyFrames(this ScalarKeyFrameAnimation animation,
        params (float frameKey, float frameValue)[] frames)
    {
        foreach (var (frameKey, frameValue) in frames)
        {
            animation.InsertKeyFrame(frameKey, frameValue);
        }
        return animation;
    }
    
    public static Vector2KeyFrameAnimation WithKeyFrames(this Vector2KeyFrameAnimation animation,
        params (float frameKey, Vector2 frameValue)[] frames)
    {
        foreach (var (frameKey, frameValue) in frames)
        {
            animation.InsertKeyFrame(frameKey, frameValue);
        }
        return animation;
    }
    
    public static Vector3KeyFrameAnimation WithKeyFrames(this Vector3KeyFrameAnimation animation,
        params (float frameKey, Vector3 frameValue)[] frames)
    {
        foreach (var (frameKey, frameValue) in frames)
        {
            animation.InsertKeyFrame(frameKey, frameValue);
        }
        return animation;
    }
}