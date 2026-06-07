using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Web;
using Cirrus.Network.Primitives;
using Cirrus.Network.Serialization;
using Cirrus.Network.Utilities;

namespace Cirrus.Network.EncryptionHandlers;

/// <summary>
/// Encryption handler using EAPI pattern.
/// </summary>
internal sealed class EapiHandler : IEncryptionHandler
{
    private static EapiHandler? _current;
    /// <summary>
    /// Current singleton instance of the EAPI handler.
    /// </summary>
    public static IEncryptionHandler Current => _current ??= new();

    private EapiHandler()
    {
        // Hidden constructor method.
    }

    /// <summary>
    /// Encrypt object using EAPI pattern.
    /// </summary>
    /// <remarks>
    /// 1. Combine the request url and the serialized object. <br/>
    /// 2. Hash the message from (1). <br/>
    /// 3. Combine the url, the serialized object and the hash from (2). <br/>
    /// 4. Encrypt the buffer from (3) with AES. <br/>
    /// 5. Return the body with only member "params", whose value is the encrypted buffer from (4).
    /// </remarks>
    /// <param name="url">The url of the request.</param>
    /// <param name="obj">The object to be encrypted.</param>
    /// <returns>The encrypted object to be sent to the server.</returns>
    private Dictionary<string, string> Encrypt(string url, Dictionary<string, object> obj)
    {
        var serialized =
            JsonSerializer.Serialize<Dictionary<string, object>>(obj,
                NetworkSerializationContext.Default.DictionaryStringObject);
        var message = $"nobody{url}use{serialized}md5forencrypt";
        var hashed = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(message))).ToLower();
        var originalBuffer = Encoding.UTF8.GetBytes($"{url}-36cd479b6b5-{serialized}-36cd479b6b5-{hashed}");
        using var aes = new AesWrapper(128, CipherMode.ECB, Constants.EapiSecretKey);
        var aesEncrypted = Convert.ToHexString(aes.Encrypt(originalBuffer));
        return new()
        {
            ["params"] = aesEncrypted
        };
    }

    /// <summary>
    /// Decrypt buffer the Secret Key from EAPI pattern.
    /// </summary>
    /// <param name="buffer">The data buffer to be decrypted.</param>
    /// <returns>The decrypted buffer.</returns>
    public static byte[] Decrypt(byte[] buffer)
    {
        // In this case, the buffer does not require decryption.
        if (buffer is [0x7b, 0x22, ..]) return buffer;
        using var aes = new AesWrapper(128, CipherMode.ECB, Constants.EapiSecretKey);
        return aes.Decrypt(buffer);
    }

    /// <summary>
    /// Handle request using EAPI pattern.
    /// </summary>
    /// <remarks>
    /// 1. Create embedded headers containing device-related info. <br/>
    /// 2. Create a "Cookie" entry, parsing the embedded headers from (1) into a single string seperated with '; ',
    /// in the headers. <br/>
    /// 3. Create a "header" field, whose value is the embedded headers from (1), in the body. <br/>
    /// 4. Encrypt the body using <see cref="Encrypt"/> method. <br/>
    /// 5. Replace the "*api" segment in the request url to "eapi".
    /// </remarks>
    /// <param name="url">Url of the request.</param>
    /// <param name="method">Method of the request.</param>
    /// <param name="headers">Headers of the request.</param>
    /// <param name="body">Body of the request.</param>
    /// <param name="cookies">Cookies of the request.</param>
    /// <param name="configUrl">Url used to encrypt the request.</param>
    /// <returns>Updated request parameters.</returns>
    public EncryptedRequest HandleRequest(string url, HttpMethod method, Dictionary<string, string> headers,
        Dictionary<string, object> body, CookieCollection cookies, string? configUrl = null)
    {
        Dictionary<string, string?> embeddedHeaders = new()
        {
            ["osver"] = cookies.FirstOrDefault(c => c.Name is "osver")?.Value ?? string.Empty,
            ["deviceId"] = cookies.FirstOrDefault(c => c.Name is "deviceId")?.Value ?? string.Empty,
            ["appver"] = cookies.FirstOrDefault(c => c.Name is "appver")?.Value ?? "8.10.10",
            ["versioncode"] = cookies.FirstOrDefault(c => c.Name is "versioncode")?.Value ?? "140",
            ["mobilename"] = cookies.FirstOrDefault(c => c.Name is "mobilename")?.Value ?? string.Empty,
            ["buildver"] = cookies.FirstOrDefault(c => c.Name is "buildver")?.Value ??
                           DateTimeOffset.Now.ToUnixTimeSeconds().ToString(),
            ["resolution"] = cookies.FirstOrDefault(c => c.Name is "resolution")?.Value ?? "1920x1080",
            ["__csrf"] = cookies.FirstOrDefault(c => c.Name is "__csrf")?.Value ?? string.Empty,
            ["os"] = cookies.FirstOrDefault(c => c.Name is "os")?.Value ?? "android",
            ["channel"] = cookies.FirstOrDefault(c => c.Name is "channel")?.Value ?? string.Empty,
            ["requestId"] = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}_{new Random().Next(1000).ToString().PadLeft(4, '0')}"
        };
        if (cookies.FirstOrDefault(c => c.Name is "MUSIC_U") is { } musicUCookie)
        {
            embeddedHeaders["MUSIC_U"] = musicUCookie.Value;
        }
        if (cookies.FirstOrDefault(c => c.Name is "MUSIC_A") is { } musicACookie)
        {
            embeddedHeaders["MUSIC_A"] = musicACookie.Value;
        }
        if (Client.UserCredentials["EApiHeaders"] is JsonObject eapiHeaders)
        {
            foreach (var kvp in eapiHeaders)
            {
                embeddedHeaders[kvp.Key] = kvp.Value?.ToString();
            }
        }
        headers["Cookie"] = string.Join("; ", embeddedHeaders.Select(p => $"{HttpUtility.HtmlEncode(p.Key)}={HttpUtility.HtmlEncode(p.Value)}"));
        body["header"] = embeddedHeaders;
        var encryptedBody = Encrypt(configUrl!, body);
        url = Regex.Replace(url, "\\w*api", "eapi");
        return new EncryptedRequest(url, headers, encryptedBody);
    }
}