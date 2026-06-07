namespace Cirrus.Utilities.Primitives;

public class ObjectRemovedEventArgs : EventArgs
{
    public object? RemovedObject { get; init; }
}