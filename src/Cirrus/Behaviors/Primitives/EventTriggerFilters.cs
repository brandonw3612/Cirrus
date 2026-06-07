using Cirrus.Generated.Attributes;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;

namespace Cirrus.Behaviors.Primitives;

internal abstract class EventTriggerFilter : DependencyObject
{
    public abstract bool Validate(object? e);
}

[ContentProperty(Name = nameof(Children))]
internal sealed class ConjunctiveEventTriggerFilter : EventTriggerFilter
{
    public List<EventTriggerFilter> Children
    {
        get => (List<EventTriggerFilter>)GetValue(ChildrenProperty);
        private init => SetValue(ChildrenProperty, value);
    }

    public static readonly DependencyProperty ChildrenProperty =
        DependencyProperty.Register(nameof(Children), typeof(List<EventTriggerFilter>),
            typeof(ConjunctiveEventTriggerFilter), new PropertyMetadata(null));

    public ConjunctiveEventTriggerFilter()
    {
        // We are not supposed to initialize the children collection in the DependencyProperty declaration.
        // Otherwise, every instance of the DependencyObject will share the same collection.
        // The constructor in DisjunctiveEventTriggerFilter stands for the same reason.
        Children = new();
    }

    public override bool Validate(object? e)
    {
        foreach (var filter in Children)
        {
            if (!filter.Validate(e)) return false;
        }
        return true;
    }
}

[ContentProperty(Name = nameof(Children))]
internal sealed class DisjunctiveEventTriggerFilter : EventTriggerFilter
{
    public List<EventTriggerFilter> Children
    {
        get => (List<EventTriggerFilter>)GetValue(ChildrenProperty);
        private init => SetValue(ChildrenProperty, value);
    }

    public static readonly DependencyProperty ChildrenProperty =
        DependencyProperty.Register(nameof(Children), typeof(List<EventTriggerFilter>),
            typeof(DisjunctiveEventTriggerFilter), new PropertyMetadata(null));

    public DisjunctiveEventTriggerFilter()
    {
        Children = new();
    }
    
    public override bool Validate(object? e)
    {
        foreach (var filter in Children)
        {
            if (filter.Validate(e)) return true;
        }
        return false;
    }
}

internal class PointerDeviceTypeAllowEventTriggerFilter : EventTriggerFilter
{
    public PointerDeviceType DeviceType
    {
        get => (PointerDeviceType)GetValue(DeviceTypeProperty);
        set => SetValue(DeviceTypeProperty, value);
    }

    public static readonly DependencyProperty DeviceTypeProperty =
        DependencyProperty.Register(nameof(DeviceType), typeof(PointerDeviceType),
            typeof(PointerDeviceTypeAllowEventTriggerFilter), new PropertyMetadata(PointerDeviceType.Mouse));

    public override bool Validate(object? e)
    {
        if (e is null) return false;
        if (EventTriggerFilterHelpers.GetPointer(e, out var pointer))
            return pointer.PointerDeviceType == DeviceType;
        if (EventTriggerFilterHelpers.GetPointerDeviceType(e, out var pointerDeviceType))
            return pointerDeviceType == DeviceType;
        return false;
    }
}

internal class PointerDeviceTypeBlockEventTriggerFilter : EventTriggerFilter
{
    public PointerDeviceType DeviceType
    {
        get => (PointerDeviceType)GetValue(DeviceTypeProperty);
        set => SetValue(DeviceTypeProperty, value);
    }

    public static readonly DependencyProperty DeviceTypeProperty =
        DependencyProperty.Register(nameof(DeviceType), typeof(PointerDeviceType),
            typeof(PointerDeviceTypeBlockEventTriggerFilter), new PropertyMetadata(PointerDeviceType.Mouse));

    public override bool Validate(object? e)
    {
        if (e is null) return false;
        if (EventTriggerFilterHelpers.GetPointer(e, out var pointer))
            return pointer.PointerDeviceType != DeviceType;
        if (EventTriggerFilterHelpers.GetPointerDeviceType(e, out var pointerDeviceType))
            return pointerDeviceType != DeviceType;
        return false;
    }
}

internal static partial class EventTriggerFilterHelpers
{
    [DuckPropertyGetter("Pointer", Assemblies = [
        "Microsoft.WinUI"
    ])]
    public static partial bool GetPointer(object args, out Pointer pointer);
    
    [DuckPropertyGetter("PointerDeviceType", Assemblies = [
        "Microsoft.WinUI"
    ])]
    public static partial bool GetPointerDeviceType(object args, out PointerDeviceType pointer);
}