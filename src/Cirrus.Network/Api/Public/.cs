using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

/// <summary>
/// Public related APIs.
/// </summary>
public sealed partial class Public : MusicApiContainer
{
    protected override string RouteBase => "/api/public";
    
    internal Public()
    {
        // Hidden constructor method.
    }
}