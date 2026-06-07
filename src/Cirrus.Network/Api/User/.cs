using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

/// <summary>
/// User related APIs.
/// </summary>
public sealed partial class User : MusicApiContainer
{
    protected override string RouteBase => "/api/user";
    
    internal User()
    {
        // Hidden constructor method.
    }
}