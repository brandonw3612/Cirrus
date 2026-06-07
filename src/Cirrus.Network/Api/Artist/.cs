using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

/// <summary>
/// Artist related APIs.
/// </summary>
public sealed partial class Artist : MusicApiContainer
{
    protected override string RouteBase => "/api/artist";
    
    internal Artist()
    {
        // Hidden constructor method.
    }
}