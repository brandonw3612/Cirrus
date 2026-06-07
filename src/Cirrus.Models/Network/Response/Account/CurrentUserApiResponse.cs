using System.Text.Json.Serialization;
using Cirrus.Models.Network.Account;
using Cirrus.Models.Network.User;

namespace Cirrus.Models.Network.Response.Account;

/// <summary>
/// Response for API account/current-user.
/// </summary>
public class CurrentUserApiResponse : MusicApiResponse
{
    /// <summary>
    /// Account information of the logged-in user.
    /// </summary>
    [JsonPropertyName("account")] public UserAccount? Account { get; init; }
    
    /// <summary>
    /// Profile of the logged-in user.
    /// </summary>
    [JsonPropertyName("profile")] public UserProfile? Profile { get; init; }
}