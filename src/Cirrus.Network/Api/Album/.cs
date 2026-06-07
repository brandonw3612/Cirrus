using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

/// <summary>
/// Album related APIs.
/// </summary>
public sealed partial class Album : MusicApiContainer
{
    protected override string RouteBase => "/api/album";
    
    internal Album()
    {
        // Hidden constructor method.
    }
}