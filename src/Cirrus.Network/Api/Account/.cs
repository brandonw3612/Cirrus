using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

/// <summary>
/// User account related APIs.
/// </summary>
public sealed partial class Account : MusicApiContainer
{
    protected override string RouteBase => "/api/account";
    
    internal Account()
    {
        // Hidden constructor method.
    }
}