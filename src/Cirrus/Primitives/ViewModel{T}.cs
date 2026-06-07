namespace Cirrus.Primitives;

public abstract class ViewModel<T> : ViewModel
{
    public T? Parameter { get; init; }
}