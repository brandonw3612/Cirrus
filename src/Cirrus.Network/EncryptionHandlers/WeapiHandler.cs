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
/// Encryption handler using WEAPI pattern.
/// </summary>
internal sealed class WeapiHandler : IEncryptionHandler
{
    private static WeapiHandler? _current;
    /// <summary>
    /// Current singleton instance of the WEAPI handler.
    /// </summary>
    public static IEncryptionHandler Current => _current ??= new();

    private WeapiHandler()
    {
        // Hidden constructor method.
    }

    /// <summary>
    /// Encrypt object using WEAPI pattern.
    /// </summary>
    /// <remarks>
    /// 1. Generate a 16-byte random secret key, using our Base62 character set. <br/>
    /// 2. Encrypt the serialized object with AES using our universal secret key. <br/>
    /// 3. Encrypt the buffer from (2) with AES using the secret key from (1). <br/>
    /// 4. Reverse the secret key from (1) and encrypt with RSA. <br/>
    /// 5. Return the body with members:
    /// "params" - the AES encryption result from (3);
    /// "encSecKey" - the RSA encryption result from (4).
    /// </remarks>
    /// <param name="obj">The object to be encrypted.</param>
    /// <returns>The encrypted object to be sent to the server.</returns>
    private Dictionary<string, string> Encrypt(Dictionary<string, object> obj)
    {
        var serialized = JsonSerializer.Serialize<Dictionary<string, object>>(obj,
            NetworkSerializationContext.Default.DictionaryStringObject);
        var randomBytes = new byte[16];
        new Random().NextBytes(randomBytes);
        var secretKey = randomBytes
            .Select(i => Convert.ToByte(Constants.Base62[i % 62]))
            .ToArray();
        using var innerAes = new AesWrapper(128, CipherMode.CBC, Constants.InnerAesSecretKey,
            Constants.AesInitializationVector);
        var innerBuffer = Encoding.UTF8.GetBytes(serialized);
        var midBuffer = Encoding.UTF8.GetBytes(Convert.ToBase64String(innerAes.Encrypt(innerBuffer)));
        using var outerAes = new AesWrapper(128, CipherMode.CBC, secretKey, Constants.AesInitializationVector);
        var aesEncrypted = Convert.ToBase64String(outerAes.Encrypt(midBuffer));
        var rsaEncrypted = Convert.ToHexString(Constants.Rsa.Encrypt(secretKey.Reverse().ToArray())).ToLower();
        return new()
        {
            ["params"] = aesEncrypted,
            ["encSecKey"] = rsaEncrypted
        };
    }

    /// <summary>
    /// Handle request using WEAPI pattern.
    /// </summary>
    /// <remarks>
    /// 1. Get "__csrf" entry from the cookies. <br/>
    /// 2. Create a "csrf_token" entry, using the value from (1), in the body. <br/>
    /// 3. Encrypt the body using <see cref="Encrypt"/> method. <br/>
    /// 4. Replace the "*api" segment in the request url to "weapi".
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
        body["csrf_token"] = cookies.FirstOrDefault(c => c.Name is "__csrf")?.Value ?? string.Empty;
        var encryptedBody = Encrypt(body);
        url = Regex.Replace(url, "\\w*api", "weapi");
        return new(url, headers, encryptedBody);
    }
}