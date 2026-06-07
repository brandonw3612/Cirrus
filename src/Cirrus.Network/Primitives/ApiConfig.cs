using System.Net;
using Cirrus.Network.Utilities;

namespace Cirrus.Network.Primitives;

/// <summary>
/// Configuration of a MusicApi.
/// </summary>
internal class ApiConfig
{
    /// <summary>
    /// Encryption handler using specific pattern to encrypt the request.
    /// </summary>
    public required IEncryptionHandler EncryptionHandler { get; init; }
    
    /// <summary>
    /// Cookies that need to be additionally put in the request.
    /// </summary>
    public CookieCollection CookiesFix { get; } = new();
    
    /// <summary>
    /// User agent platform of the API, which id randomly selected when requests are sent.
    /// </summary>
    public UserAgents.Platform UserAgentPlatform { get; init; } = UserAgents.Platform.Unspecified;
    
    /// <summary>
    /// Url used to encrypt requests. Only used in EAPI pattern.
    /// </summary>
    public string? Url { get; init; }
}