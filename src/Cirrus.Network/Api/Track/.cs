using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

/// <summary>
/// Track related APIs.
/// </summary>
public sealed partial class Track : MusicApiContainer
{
    protected override string RouteBase => "/api/track";
    
    internal Track()
    {
        // Hidden constructor method.
    }
}