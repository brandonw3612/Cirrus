namespace Cirrus.Models.Business.Network;

/// <summary>
/// Proxy mode of the networking client.
/// </summary>
public enum ProxyMode
{
    /// <summary>
    /// Ignore system proxy settings.
    /// </summary>
    NoProxy = 0,
    /// <summary>
    /// Use system proxy settings.
    /// </summary>
    SystemProxy = 1,
    /// <summary>
    /// Use custom proxy settings.
    /// </summary>
    CustomProxy = 2
}