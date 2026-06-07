using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;

namespace Cirrus.Commanding.Primitives;

public interface IRelayCommandEx;

/// <summary>
///     A command whose sole purpose is to relay its functionality to other
///     objects by invoking delegates. The default return value for the <see cref="CanExecute" />
///     method is <see langword="true" />. This type does not allow you to accept command parameters
///     in the <see cref="Execute" /> and <see cref="CanExecute" /> callback methods.
/// </summary>
public sealed partial class RelayCommandEx<TSender, TEventArgs> : IRelayCommand, IRelayCommandEx
{
    /// <summary>
    ///     The optional action to invoke when <see cref="CanExecute" /> is used.
    /// </summary>
    private readonly Func<TSender, TEventArgs, bool>? _canExecute;

    /// <summary>
    ///     The <see cref="Action" /> to invoke when <see cref="Execute" /> is used.
    /// </summary>
    private readonly Action<TSender, TEventArgs> _execute;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RelayCommand" /> class that can always execute.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="execute" /> is <see langword="null" />.</exception>
    public RelayCommandEx(Action<TSender, TEventArgs> execute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RelayCommand" /> class.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    /// <param name="canExecute">The execution status logic.</param>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if <paramref name="execute" /> or <paramref name="canExecute" />
    ///     are <see langword="null" />.
    /// </exception>
    public RelayCommandEx(Action<TSender, TEventArgs>? execute, Func<TSender, TEventArgs, bool>? canExecute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
    }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc />
    public void NotifyCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanExecute(object? parameter)
    {
        if (parameter is not ValueTuple<object, object> composedParameter) return false;
        if (composedParameter.Item1 is not TSender sender || composedParameter.Item2 is not TEventArgs args)
            return false;
        return _canExecute?.Invoke(sender, args) != false;
    }

    public void Execute(object? parameter)
    {
        if (parameter is not ValueTuple<object, object> composedParameter) return;
        if (composedParameter.Item1 is not TSender sender || composedParameter.Item2 is not TEventArgs args) return;
        _execute(sender, args);
    }
}