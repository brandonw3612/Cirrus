using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

/// <summary>
/// Playlist related APIs.
/// </summary>
public sealed partial class Playlist : MusicApiContainer
{
    protected override string RouteBase => "/api/playlist";
    
    internal Playlist()
    {
        // Hidden constructor method.
    }
}