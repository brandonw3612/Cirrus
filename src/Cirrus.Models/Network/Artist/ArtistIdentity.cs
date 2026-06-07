using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Cirrus.Models.Network.Artist;

/// <summary>
/// Identity of an artist.
/// </summary>
[DebuggerDisplay("Title: {Title}, ID: {IdentityId}")]
public class ArtistIdentity
{
    /// <summary>
    /// Identity ID, defined by NetEase.
    /// </summary>
    [JsonPropertyName("expertIdentiyId")] public int IdentityId { get; init; }
    
    /// <summary>
    /// Title of the identity.
    /// </summary>
    [JsonPropertyName("expertIdentiyName")] public string Title { get; init; } = "Unknown Identity";
    
    /// <summary>
    /// Count of work by the artist, corresponding to the identity.
    /// </summary>
    [JsonPropertyName("expertIdentiyCount")] public int RelatedWorkCount { get; init; }
}