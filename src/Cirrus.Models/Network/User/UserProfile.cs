using System.Diagnostics;
using System.Text.Json.Serialization;
using Cirrus.Models.Abstract;

namespace Cirrus.Models.Network.User;

/// <summary>
/// Profile of a user.
/// </summary>
[DebuggerDisplay("Nickname: {Nickname}, ID: {UserId}")]
public class UserProfile : IUser
{
    /// <summary>
    /// Url of the user's avatar image.
    /// </summary>
    [JsonPropertyName("avatarUrl")] public string? AvatarImageUrl { get; init; }

    /// <summary>
    /// Url of the background image on the user's homepage.
    /// </summary>
    [JsonPropertyName("backgroundUrl")] public string? BackgroundImageUrl { get; init; }
    
    /// <summary>
    /// City code of the user, presented in an integer, defined by NetEase.
    /// </summary>
    [JsonPropertyName("city")] public int CityCode { get; init; }
    
    /// <summary>
    /// Unix timestamp that the user registered the account.
    /// </summary>
    [JsonPropertyName("createTime")] public long RegisteredTimestamp { get; init; }
    
    /// <summary>
    /// Total count of posts the user has shared on the platform.
    /// </summary>
    [JsonPropertyName("eventCount")] public int PostCount { get; init; }
    
    /// <summary>
    /// Whether the logged-in user is following the current user.
    /// </summary>
    [JsonPropertyName("followed")] public bool IsFollowing { get; init; }
    
    /// <summary>
    /// Total count of the user's followers.
    /// </summary>
    [JsonPropertyName("followeds")] public int FollowerCount { get; init; }
    
    /// <summary>
    /// Whether current user is following the logged-in user.
    /// </summary>
    [JsonPropertyName("followMe")] public bool IsFollower { get; init; }
    
    /// <summary>
    /// Total count of the user's following list.
    /// </summary>
    [JsonPropertyName("follows")] public int FollowingCount { get; init; }
    
    /// <summary>
    /// If the logged-in user is following the current user, this value indicates the time of the following status; <br/>
    /// Otherwise, this value is null.
    /// </summary>
    [JsonPropertyName("followTime")] public string? FollowedTime { get; init; }
    
    /// <summary>
    /// Gender of the user, presented in an integer, defined by NetEase.
    /// </summary>
    // TODO: Classification / Documentation needed.
    [JsonPropertyName("gender")] public int Gender { get; init; }
    
    /// <summary>
    /// Nickname of the user.
    /// </summary>
    [JsonPropertyName("nickname")] public string Nickname { get; init; } = string.Empty;
    
    /// <summary>
    /// Total count of subscribers to current user's playlists.
    /// </summary>
    [JsonPropertyName("playlistBeSubscribedCount")] public long PlaylistSubscriberCount { get; init; }
    
    /// <summary>
    /// Total count of current user's playlists.
    /// </summary>
    [JsonPropertyName("playlistCount")] public int PlaylistCount { get; init; }
    
    /// <summary>
    /// Province code of the user, presented in an integer, defined by NetEase.
    /// </summary>
    [JsonPropertyName("province")] public int ProvinceCode { get; init; }
    
    /// <summary>
    /// Contact name of the current user, set by the logged-in user.
    /// </summary>
    [JsonPropertyName("remarkName")] public string? ContactName { get; init; }
    
    /// <summary>
    /// Bio of current user.
    /// </summary>
    [JsonPropertyName("signature")] public string Bio { get; init; } = string.Empty;
    
    /// <summary>
    /// ID of the user, defined by NetEase when registered.
    /// </summary>
    [JsonPropertyName("userId")] public ulong UserId { get; init; }
    
    /// <summary>
    /// Type of the user, presented in an integer, defined by NetEase.
    /// </summary>
    [JsonPropertyName("userType")] public int UserType { get; init; }
    
    /// <summary>
    /// Membership type of the user, presented in an integer, defined by NetEase.
    /// </summary>
    [JsonPropertyName("vipType")] public int MembershipType { get; init; }
}