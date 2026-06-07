using System.Text.Json.Serialization;
using Cirrus.Models.Network.User;
using Cirrus.Models.Shared.Search;

namespace Cirrus.Models.Network.Response.Search;

/// <summary>
/// Response for API search/cloud-search when search target is <see cref="SearchTarget.User"/>.
/// </summary>
public class UserSearchApiResponse : SearchApiResponse, IJsonOnDeserialized
{
    /// <summary>
    /// Users matching with the keyword.
    /// </summary>
    [JsonIgnore] public List<UserProfile> Users { get; private set; } = new();
    
    /// <summary>
    /// Total count of users that match with the keyword.
    /// </summary>
    [JsonIgnore] public int TotalCount { get; private set; }
    
    [JsonInclude] [JsonPropertyName("result")] internal UserSearchResult? Result { get; init; }

    void IJsonOnDeserialized.OnDeserialized()
    {
        if (Result is null) return;
        Users = Result.Users;
        TotalCount = Result.TotalCount;
    }

    public class UserSearchResult
    {
        [JsonPropertyName("userprofiles")] public List<UserProfile> Users { get; init; } = new();
        [JsonPropertyName("userprofileCount")] public int TotalCount { get; init; }
    }
}