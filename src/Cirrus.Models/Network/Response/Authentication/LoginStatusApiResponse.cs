using System.Text.Json.Serialization;
using Cirrus.Models.Network.Account;
using Cirrus.Models.Network.User;

namespace Cirrus.Models.Network.Response.Authentication;

/// <summary>
/// Response for API authentication/login-status.
/// </summary>
public class LoginStatusApiResponse : MusicApiResponse
{
    /// <summary>
    /// Account of the current user.
    /// </summary>
    [JsonPropertyName("account")] public UserAccount? Account { get; init; }
    
    /// <summary>
    /// Profile of the current user.
    /// </summary>
    [JsonPropertyName("profile")] public UserProfile? Profile { get; init; }

    
    /// <summary>
    /// Whether a user has logged in.
    /// </summary>
    [JsonIgnore] public bool IsLoggedIn => !(Profile is null || Account is null || Account is { Status: -10 });
}