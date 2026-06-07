namespace Cirrus.Network.Api;

/// <summary>
/// Constants used by APIs.
/// </summary>
public static class Constants
{
    /// <summary>
    /// Current protocol used by the client to make requests.
    /// </summary>
    public static string Protocol { get; set; } = "https";
    
    /// <summary>
    /// Request base of URLs, using the domain name "music.163.com".
    /// </summary>
    internal static string RequestBase => $"{Protocol}://music.163.com";
    
    /// <summary>
    /// Request base of URLs, using the domain name "interface.music.163.com".
    /// </summary>
    internal static string InterfaceRequestBase => $"{Protocol}://interface.music.163.com";
    
    /// <summary>
    /// Request base of URLs, using the domain name "interface3.music.163.com".
    /// </summary>
    internal static string Interface3RequestBase => $"{Protocol}://interface3.music.163.com";
}