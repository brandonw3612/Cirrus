using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Cirrus.Models.Network.Response;

/// <summary>
/// Base response object for music APIs.
/// </summary>
[DebuggerDisplay("Code: {StatusCode}, Message: {ResponseMessage}")]
public class MusicApiResponse
{
    /// <summary>
    /// Status code indicating whether the request succeeded.
    /// </summary>
    [JsonPropertyName("code")] public int StatusCode { get; init; }
    
    /// <summary>
    /// Additional message sent back from the server,
    /// usually containing error information when the request did not succeed.
    /// </summary>
    [JsonPropertyName("msg")] public string ResponseMessage { get; init; } = string.Empty;
}