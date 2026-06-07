using System.Diagnostics;
using System.Text.Json.Serialization;
using Cirrus.Models.Abstract;

namespace Cirrus.Models.Network.Artist;

/// <summary>
/// Detail of an artist.
/// </summary>
[DebuggerDisplay("Name: {Name}, ID: {ArtistId}")]
public class ArtistDetail : IArtist
{
    /// <summary>
    /// Whether the current user is following the artist.
    /// </summary>
    [JsonPropertyName("followed")] public bool IsFollowing{ get; init; }
    
    /// <summary>
    /// Name alias of the artist.
    /// </summary>
    [JsonPropertyName("alias")] public List<string> Alias { get; init; } = new();
    
    /// <summary>
    /// Translation for the name of the artist.
    /// </summary>
    [JsonPropertyName("trans")] public string? NameTranslation { get; init; }
    
    /// <summary>
    /// Total count of tracks performed by the current artist.
    /// </summary>
    [JsonPropertyName("musicSize")] public int TrackCount { get; init; }
    
    /// <summary>
    /// Total count of albums performed by the current artist.
    /// </summary>
    [JsonPropertyName("albumSize")] public int AlbumCount { get; init; }
    
    /// <summary>
    /// Brief description of the artist.
    /// </summary>
    [JsonPropertyName("briefDesc")] public string? BriefDescription { get; init; }
    
    /// <summary>
    /// Url of the artist's avatar image.
    /// </summary>
    [JsonPropertyName("picUrl")] public string? AvatarImageUrl { get; init; }
    
    /// <summary>
    /// Url of the artist's squared avatar image.
    /// </summary>
    [JsonPropertyName("img1v1Url")] public string? SquaredAvatarImageUrl { get; init; }
    
    /// <summary>
    /// Name of the artist.
    /// </summary>
    [JsonPropertyName("name")] public string Name { get; init; } = "Unknown Artist";
    
    /// <summary>
    /// ID of the artist, defined by NetEase.
    /// </summary>
    [JsonPropertyName("id")] public ulong ArtistId { get; init; }
}

/// <summary>
/// Detail of an artist.
/// </summary>
[DebuggerDisplay("Name: {Name}, ID: {ArtistId}")]
public class ArtistDetail2 : IArtist
{
    /// <summary>
    /// ID of the artist, defined by NetEase.
    /// </summary>
    [JsonPropertyName("id")] public ulong ArtistId { get; init; }
    
    /// <summary>
    /// Url of the artist's avatar image.
    /// </summary>
    [JsonPropertyName("cover")] public string? AvatarImageUrl { get; init; }
    
    /// <summary>
    /// Url of the artist's squared avatar image.
    /// </summary>
    [JsonPropertyName("avatar")] public string? SquaredAvatarImageUrl { get; init; }
    
    /// <summary>
    /// Name of the artist.
    /// </summary>
    [JsonPropertyName("name")] public string Name { get; init; } = "Unknown Artist";
    
    /// <summary>
    /// Translations for the name of the artist.
    /// </summary>
    [JsonPropertyName("transNames")] public List<string> NameTranslations { get; init; } = new();
    
    /// <summary>
    /// Name alias of the artist.
    /// </summary>
    [JsonPropertyName("alias")] public List<string> Alias { get; init; } = new();
    
    /// <summary>
    /// Brief description of the artist.
    /// </summary>
    [JsonPropertyName("briefDesc")] public string? BriefDescription { get; init; }
    
    /// <summary>
    /// Total count of albums performed by the current artist.
    /// </summary>
    [JsonPropertyName("albumSize")] public int AlbumCount { get; init; }
    
    /// <summary>
    /// Total count of tracks performed by the current artist.
    /// </summary>
    [JsonPropertyName("musicSize")] public int TrackCount { get; init; }
    
    /// <summary>
    /// Total count of music videos performed by the current artist.
    /// </summary>
    [JsonPropertyName("mvSize")] public int MusicVideoCount { get; init; }
}