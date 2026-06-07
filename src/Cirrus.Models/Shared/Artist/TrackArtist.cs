using System.Diagnostics;
using System.Text.Json.Serialization;
using Cirrus.Models.Abstract;

namespace Cirrus.Models.Shared.Artist;

/// <summary>
/// Artist brief information as the property of a track.
/// </summary>
[DebuggerDisplay("Name: {Name}, ID: {ArtistId}")]
public class TrackArtist : IArtist
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
    /// Translations for the name of the artist.
    /// </summary>
    [JsonPropertyName("tns")] public List<string> NameTranslations { get; init; } = new();
    
    /// <summary>
    /// Name alias of the artist.
    /// </summary>
    [JsonPropertyName("alias")] public List<string> Alias { get; init; } = new();
}