using System.Net;

namespace Cirrus.Network.Primitives;

/// <summary>
/// Request parameters encrypted using specific pattern.
/// </summary>
/// <param name="RequestUrl">Url of the updated request.</param>
/// <param name="RequestHeaders">Headers of the updated request.</param>
/// <param name="RequestBody">Body of the updated request.</param>
internal record EncryptedRequest(string RequestUrl, Dictionary<string, string> RequestHeaders,
    Dictionary<string, string> RequestBody);

/// <summary>
/// Encryption handler using specific pattern.
/// </summary>
internal interface IEncryptionHandler
{
    /// <summary>
    /// Current singleton instance of the encryption handler.
    /// </summary>
    public static abstract IEncryptionHandler Current { get; }

    /// <summary>
    /// Handle request using specific pattern.
    /// </summary>
    /// <remarks>
    /// Might update request URL and headers. Must update request body.
    /// </remarks>
    /// <param name="url">Url of the request.</param>
    /// <param name="method">Method of the request.</param>
    /// <param name="headers">Headers of the request.</param>
    /// <param name="body">Body of the request.</param>
    /// <param name="cookies">Cookies of the request.</param>
    /// <param name="configUrl">Url used to encrypt the request. Only used in EAPI pattern.</param>
    /// <returns>Updated request parameters.</returns>
    public EncryptedRequest HandleRequest(string url, HttpMethod method, Dictionary<string, string> headers,
        Dictionary<string, object> body, CookieCollection cookies, string? configUrl = null);
}