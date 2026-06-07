using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Cirrus.Models.Network.Artist;

/// <summary>
/// Artist brief information as the property of a music video.
/// </summary>
[DebuggerDisplay("Name: {Name}, ID: {ArtistId}")]
public class MusicVideoArtist
{
    /// <summary>
    /// ID of the artist, defined by NetEase.
    /// </summary>
    [JsonPropertyName("id")] public ulong ArtistId { get; init; }
    
    /// <summary>
    /// Name of the artist.
    /// </summary>
    [JsonPropertyName("name")] public string Name { get; init; } = "Unknown Artist";
    
    /// <summary>
    /// Name alias of the artist.
    /// </summary>
    [JsonPropertyName("alias")] public List<string> Alias { get; init; } = new();
    
    /// <summary>
    /// Translations for the name of the artist.
    /// </summary>
    [JsonPropertyName("transNames")] public List<string> NameTranslations { get; init; } = new();
}