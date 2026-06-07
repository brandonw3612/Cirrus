using Cirrus.Behaviors.Primitives;
using Cirrus.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Cirrus.Behaviors;

internal class LoadedEventTriggerBehavior : EventTriggerBehaviorBase<FrameworkElement>
{
    private bool _isLoadedEventRegistered;
    
    protected override bool RegisterEventCore(FrameworkElement? source)
    {
        if (_isLoadedEventRegistered) return true;
        if (source is null || EventTriggerBehaviorHelpers.IsElementLoaded(source)) return true;
        _isLoadedEventRegistered = true;
        source.Loaded += OnEvent;
        return true;
    }

    protected override void UnregisterEventCore(FrameworkElement? source)
    {
        if (!_isLoadedEventRegistered) return;
        _isLoadedEventRegistered = false;
        source!.Loaded -= OnEvent;
    }
}

internal static class EventTriggerBehaviorHelpers
{
    // This method has to be outside 'EventTriggerBehavior', because it's actually trim-safe.
    // We want to allow other callers inside the library use this without getting trim warnings.
    public static bool IsElementLoaded(FrameworkElement? element)
    {
        if (element == null)
        {
            return false;
        }

        UIElement? rootVisual = null;
        if (element.XamlRoot != null)
        {
            rootVisual = element.XamlRoot.Content;
        }
        else if (MainWindow.Current != null)
        {
            rootVisual = MainWindow.Current.Content;
        }

        var parent = element.Parent;
        if (parent == null)
        {
            // If the element is the child of a ControlTemplate it will have a null parent even when it is loaded.
            // To catch that scenario, also check its parent in the visual tree.
            parent = VisualTreeHelper.GetParent(element);
        }

        return parent != null || (rootVisual != null && element == rootVisual);
    }
}