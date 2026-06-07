using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

/// <summary>
/// Search related APIs.
/// </summary>
public sealed partial class Search : MusicApiContainer
{
    protected override string RouteBase => "/api/search";
    
    internal Search()
    {
        // Hidden constructor method.
    }
}