using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Cirrus.Network.Primitives;
using Cirrus.Network.Serialization;
using Cirrus.Network.Utilities;

namespace Cirrus.Network.EncryptionHandlers;

/// <summary>
/// Encryption handler using LinuxAPI pattern.
/// </summary>
internal sealed class LinuxApiHandler : IEncryptionHandler
{
    private static LinuxApiHandler? _current;
    /// <summary>
    /// Current singleton instance of the LinuxAPI handler.
    /// </summary>
    public static IEncryptionHandler Current => _current ??= new();

    private LinuxApiHandler()
    {
        // Hidden constructor method.
    }

    /// <summary>
    /// Encrypt object using LinuxAPI pattern.
    /// </summary>
    /// <remarks>
    /// 1. Encrypt the serialized object with AES. <br/>
    /// 2. Return the body with only member "eparams", whose value if the encrypted buffer from (1).
    /// </remarks>
    /// <param name="obj">The object to be encrypted.</param>
    /// <returns>The encrypted object to be sent to the server.</returns>
    private Dictionary<string, string> Encrypt(Dictionary<string, object> obj)
    {
        var serialized = JsonSerializer.Serialize<Dictionary<string, object>>(obj, NetworkSerializationContext.Default.DictionaryStringObject);
        using var aes = new AesWrapper(128, CipherMode.ECB, Constants.LinuxApiSecretKey);
        var aesEncrypted = Convert.ToHexString(aes.Encrypt(Encoding.UTF8.GetBytes(serialized)));
        return new()
        {
            ["eparams"] = aesEncrypted
        };
    }

    /// <summary>
    /// Handle request using LinuxAPI pattern.
    /// </summary>
    /// <remarks>
    /// 1. Replace the "*api" segment in the request url to "api". <br/>
    /// 2. Create a dictionary containing following entries: <br/>
    ///     method - Method of the request; <br/>
    ///     url - Updated url from (1); <br/>
    ///     params - The original body; <br/>
    /// 3. Encrypt the content from (2); <br/>
    /// 4. Create a "User-Agent" in the headers using Linux Chrome User Agent.
    /// 5. Change the url to "https://music.163.com/api/linux/forward".
    /// </remarks>
    /// <param name="url">Url of the request.</param>
    /// <param name="method">Method of the request.</param>
    /// <param name="headers">Headers of the request.</param>
    /// <param name="body">Body of the request.</param>
    /// <param name="cookies">Cookies of the request.</param>
    /// <param name="_">Ignored parameter.</param>
    /// <returns>Updated request parameters.</returns>
    public EncryptedRequest HandleRequest(string url, HttpMethod method, Dictionary<string, string> headers,
        Dictionary<string, object> body, CookieCollection cookies, string? _ = null)
    {
        url = Regex.Replace(url, "\\w*api", "api");
        var encryptedBody = Encrypt(new Dictionary<string, object>
        {
            ["method"] = method.Method.ToLower(),
            ["url"] = url,
            ["params"] = body
        });
        headers["User-Agent"] = UserAgents.LinuxChrome;
        url = "https://music.163.com/api/linux/forward";
        return new(url, headers, encryptedBody);
    }
}