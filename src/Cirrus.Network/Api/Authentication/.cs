using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

/// <summary>
/// Authentication related APIs.
/// </summary>
public sealed partial class Authentication : MusicApiContainer
{
    protected override string RouteBase => "/api/authentication";
    
    internal Authentication()
    {
        // Hidden constructor method.
    }
}