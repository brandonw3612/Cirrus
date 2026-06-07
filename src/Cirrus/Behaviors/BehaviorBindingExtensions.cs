using Cirrus.Behaviors.Primitives;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.Xaml.Interactivity;

namespace Cirrus.Behaviors;

internal static class BehaviorBindingExtensions
{
    public static void AttachEventTriggerBehaviorToCommand<S, T>(this DependencyObject dependencyObject, IRelayCommand command)
        where S : DependencyObject
        where T : EventTriggerBehaviorBase<S>, new()
    {
        var collection = Interaction.GetBehaviors(dependencyObject);
        collection.Add(new T
        {
            Actions =
            {
                new InvokeCommandActionEx
                {
                    Command = command
                }
            }
        });
        Interaction.SetBehaviors(dependencyObject, collection);
    }

    public static void DetachBehavior<T>(this DependencyObject dependencyObject) where T : Behavior
    {
        var collection = Interaction.GetBehaviors(dependencyObject);
        var behaviorsToRemove = collection.OfType<T>().ToList();
        behaviorsToRemove.ForEach(b => collection.Remove(b));
    }
}