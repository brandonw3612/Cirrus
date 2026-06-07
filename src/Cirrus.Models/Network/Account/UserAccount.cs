using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Cirrus.Models.Network.Account;

/// <summary>
/// User account information.
/// </summary>
[DebuggerDisplay("Nickname: {Nickname}, ID: {UserId}")]
public class UserAccount
{
    /// <summary>
    /// ID of the user, defined by NetEase when registered.
    /// </summary>
    [JsonPropertyName("id")] public ulong UserId { get; init; }

    /// <summary>
    /// Nickname of the user.
    /// </summary>
    [JsonPropertyName("userName")] public string Nickname { get; init; } = "Unknown User";
    
    /// <summary>
    /// Type of the user, presented in an integer, defined by NetEase.
    /// </summary>
    [JsonPropertyName("type")] public int Type { get; init; }
    
    /// <summary>
    /// Status of the user, presented in an integer, defined by NetEase.
    /// </summary>
    /// <remarks>-10 - user is logged out.</remarks>
    [JsonPropertyName("status")] public int Status { get; init; }
    
    /// <summary>
    /// Unix timestamp that the user registered the account.
    /// </summary>
    [JsonPropertyName("createTime")] public long RegisteredTimestamp { get; init; }
    
    /// <summary>
    /// Membership type of the user, presented in an integer, defined by NetEase.
    /// </summary>
    [JsonPropertyName("vipType")] public int MembershipType { get; init; }
}