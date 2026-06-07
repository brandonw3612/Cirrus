using System.Text.Json.Serialization;
using Cirrus.Models.Network.User;

namespace Cirrus.Models.Network.Response.Search.GeneralSearchModules;

/// <summary>
/// User module in general search result.
/// </summary>
public class UserSearchResultModule : SearchModuleBase
{
    /// <summary>
    /// Users matching with the keyword.
    /// </summary>
    [JsonPropertyName("users")] public List<UserProfile> Users { get; init; } = new();
    
    /// <summary>
    /// Whether there are more matched users other than current list.
    /// </summary>
    [JsonPropertyName("more")] public bool HasMore { get; init; }
    
    /// <summary>
    /// Caption on the "More" Button.
    /// </summary>
    [JsonPropertyName("moreText")] public string? MoreButtonCaption { get; init; }
}