using System.Diagnostics;
using System.Text.Json.Serialization;
using Cirrus.Models.Abstract;

namespace Cirrus.Models.Shared.Album;

/// <summary>
/// Album brief information as the property of a track.
/// </summary>
[DebuggerDisplay("Title: {Title}, ID: {AlbumId}")]
public class TrackAlbum : IAlbum
{
    /// <summary>
    /// ID of the album, defined by NetEase.
    /// </summary>
    [JsonPropertyName("id")] public ulong AlbumId { get; init; }

    /// <summary>
    /// Title of the album.
    /// </summary>
    [JsonPropertyName("name")] public string Title { get; init; } = "Unknown Album";
    
    /// <summary>
    /// Url of the album's artwork image.
    /// </summary>
    [JsonPropertyName("picUrl")] public string? ArtworkImageUrl { get; init; }
    
    /// <summary>
    /// Translations for the title of the album.
    /// </summary>
    [JsonPropertyName("tns")] public List<string> TitleTranslations { get; init; } = new();
}