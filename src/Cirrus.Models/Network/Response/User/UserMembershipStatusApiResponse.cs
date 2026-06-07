using System.Text.Json.Serialization;
using Cirrus.Models.Network.User;
using Cirrus.Models.Shared.User;

namespace Cirrus.Models.Network.Response.User;

/// <summary>
/// Response for API user/membership-status.
/// </summary>
public class UserMembershipStatusApiResponse : MusicApiResponse, IJsonOnDeserialized
{
    /// <summary>
    /// ID of the user.
    /// </summary>
    [JsonIgnore] public long UserId { get; private set; }
    
    /// <summary>
    /// Premium level of the user.
    /// </summary>
    [JsonIgnore] public int PremiumLevel { get; private set; }
    
    /// <summary>
    /// Package membership status of the user.
    /// </summary>
    [JsonIgnore] public MembershipStatus? Package { get; private set; }
    
    /// <summary>
    /// Premium membership status of the user.
    /// </summary>
    [JsonIgnore] public MembershipStatus? Premium { get; private set; }
    
    /// <summary>
    /// Premium Plus membership status of the user.
    /// </summary>
    [JsonIgnore] public MembershipStatus? PremiumPlus { get; private set; }

    /// <summary>
    /// Membership type of the user.
    /// </summary>
    [JsonIgnore] public MembershipType MembershipType =>
        PremiumPlus is { IsExpired: false } ? MembershipType.PremiumPlus :
        Premium is { IsExpired: false } ? MembershipType.Premium :
        Package is { IsExpired: false } ? MembershipType.Package : MembershipType.None;
    
    [JsonInclude] [JsonPropertyName("data")] internal MembershipData? Data { get; init; }

    void IJsonOnDeserialized.OnDeserialized()
    {
        if (Data is null) return;
        UserId = Data.UserId;
        PremiumLevel = Data.PremiumLevel;
        Package = Data.Package;
        Premium = Data.Premium;
        PremiumPlus = Data.PremiumPlus;
    }
    
    public class MembershipData
    {
        [JsonPropertyName("redVipLevel")] public int PremiumLevel { get; init; }
        [JsonPropertyName("musicPackage")] public MembershipStatus? Package { get; init; }
        [JsonPropertyName("associator")] public MembershipStatus? Premium { get; init; }
        [JsonPropertyName("userId")] public long UserId { get; init; }
        [JsonPropertyName("redplus")] public MembershipStatus? PremiumPlus { get; init; }
    }
}