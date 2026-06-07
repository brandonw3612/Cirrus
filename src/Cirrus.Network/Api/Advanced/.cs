using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

/// <summary>
/// Advanced APIs.
/// </summary>
public sealed partial class Advanced : MusicApiContainer
{
    protected override string RouteBase => "/api/advanced";
    
    internal Advanced()
    {
        // Hidden constructor method.
    }
}