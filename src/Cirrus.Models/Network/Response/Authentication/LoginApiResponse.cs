using System.Text.Json.Serialization;
using Cirrus.Models.Network.Account;
using Cirrus.Models.Network.User;

namespace Cirrus.Models.Network.Response.Authentication;

/// <summary>
/// Response for API authentication/login/mail and authentication/login/phone/password.
/// </summary>
public class LoginApiResponse : MusicApiResponse
{
    /// <summary>
    /// Type of the login behavior, classified by NetEase.
    /// </summary>
    [JsonPropertyName("loginType")] public int LoginType { get; init; }
    
    /// <summary>
    /// Account information of the logged-in user.
    /// </summary>
    [JsonPropertyName("account")] public UserAccount? Account { get; init; }
    
    /// <summary>
    /// Token for the logged-in user.
    /// </summary>
    [JsonPropertyName("token")] public string? LoginToken { get; init; }
    
    /// <summary>
    /// Profile of the logged-in user.
    /// </summary>
    [JsonPropertyName("profile")] public UserProfile? Profile { get; init; }
}