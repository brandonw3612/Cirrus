namespace Cirrus.Network.Primitives;

/// <summary>
/// Music API container definition. Entry point of a specific group of APIs.
/// </summary>
public abstract class MusicApiContainer
{
    /// <summary>
    /// Route base shared by the APIs in the container.
    /// </summary>
    protected abstract string RouteBase { get; }
}