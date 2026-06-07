using System.Net.Http.Headers;

namespace Cirrus.Network.Primitives;

/// <summary>
/// Constants used by primitives.
/// </summary>
internal static class Constants
{
    /// <summary>
    /// Token when no user is logged in.
    /// </summary>
    public const string AnonymousToken = "bf8bfeabb1aa84f9c8c3906c04a04fb864322804c83f5d607e91a04eae463c9436bd1a17ec353cf780b396507a3f7464e8a60f4bbc019437993166e004087dd32d1490298caf655c2353e58daa0bc13cc7d5c198250968580b12c1b8817e3f5c807e650dd04abd3fb8130b7ae43fcc5b";

    /// <summary>
    /// Available HTTP request content headers.
    /// </summary>
    public static List<string> ContentHeaderNames => field ??=
        typeof(HttpContentHeaders).GetProperties().Select(p => p.Name.ToLower()).ToList();

    /// <summary>
    /// Status codes that indicate success responses.
    /// </summary>
    public static List<int> SuccessStatusCodes => field ??= [200, 201, 302, 400, 502, 800, 801, 802, 803];
}