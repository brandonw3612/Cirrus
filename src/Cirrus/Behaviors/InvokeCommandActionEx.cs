using System.Windows.Input;
using Cirrus.Commanding.Primitives;
using Microsoft.UI.Xaml;
using Microsoft.Xaml.Interactivity;

namespace Cirrus.Behaviors;

/// <summary>
/// Executes a specified <see cref="ICommand" /> when invoked.
/// </summary>
public sealed class InvokeCommandActionEx : DependencyObject, IAction
{
    /// <summary>
    /// Identifies the <seealso cref="Command" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command),
        typeof(ICommand), typeof(InvokeCommandActionEx), new PropertyMetadata(null));

    /// <summary>
    /// Identifies the <seealso cref="CommandParameter"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(nameof(CommandParameter),
        typeof(object), typeof(InvokeCommandActionEx), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the command this action should invoke. This is a dependency property.
    /// </summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter that is passed to <see cref="ICommand.Execute(object)"/>.
    /// If this is not set, the parameter from the <seealso cref="Execute(object, object)"/> method will be used.
    /// This is an optional dependency property.
    /// </summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>
    /// Executes the action.
    /// </summary>
    /// <param name="sender">
    /// The <see cref="object" /> that is passed to the action by the behavior. Generally this is
    /// <seealso cref="IBehavior.AssociatedObject" /> or a target object.
    /// </param>
    /// <param name="parameter">The value of this parameter is determined by the caller.</param>
    /// <returns>True if the command is successfully executed; else false.</returns>
    public object Execute(object sender, object parameter)
    {
        var realCommand = Command switch
        {
            null => null,
            CommandWrapper wrapper => wrapper.InnerCommand,
            _ => Command
        };
        if (CommandParameter is not null)
        {
            if (realCommand?.CanExecute(CommandParameter) is not true) return false;
            realCommand.Execute(CommandParameter);
            return true;
        }
        switch (realCommand)
        {
            case null:
                return false;
            case IRelayCommandEx:
                {
                    var typeArguments = realCommand.GetType().GetGenericArguments();
                    if (typeArguments.Length != 2) return false;
                    if (sender.GetType() != typeArguments[0] && !sender.GetType().IsAssignableTo(typeArguments[0]))
                        return false;
                    if (parameter.GetType() != typeArguments[1] && !parameter.GetType().IsAssignableTo(typeArguments[1]))
                        return false;
                    object composedArgument = (sender, parameter);
                    if (!realCommand.CanExecute(composedArgument)) return false;
                    realCommand.Execute(composedArgument);
                    return true;
                }
            default:
                {
                    if (!realCommand.CanExecute(parameter)) return false;
                    realCommand.Execute(parameter);
                    return true;
                }
        }
    }
}