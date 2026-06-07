using System.Diagnostics;
using System.Text.Json.Serialization;
using Cirrus.Models.Abstract;
using Cirrus.Models.Business.Album;
using Cirrus.Models.Network.Artist;
using Cirrus.Models.Network.Track;

namespace Cirrus.Models.Network.Album;

/// <summary>
/// Detail of an album.
/// </summary>
[DebuggerDisplay("Title: {Title}, ID: {AlbumId}")]
public class AlbumDetail : IAlbum
{
    /// <summary>
    /// Tracks on the album.
    /// </summary>
    [JsonPropertyName("songs")] public List<TrackDetail> Tracks { get; init; } = new();
    
    /// <summary>
    /// Whether the album is purchased.
    /// </summary>
    [JsonPropertyName("paid")] public bool IsPurchased { get; init; }
    
    /// <summary>
    /// Whether the album is on sale.
    /// </summary>
    [JsonPropertyName("onSale")] public bool IsOnSale { get; init; }
    
    /// <summary>
    /// Special marks of the album, defined as an integer number by NetEase.
    /// </summary>
    [JsonPropertyName("mark")] public int SpecialMarks { get; init; }
    
    /// <summary>
    /// Alias of the album title.
    /// </summary>
    [JsonPropertyName("alias")] public List<string> Alias { get; init; } = new();
    
    /// <summary>
    /// Artists of the album.
    /// </summary>
    [JsonPropertyName("artists")] public List<ArtistDetail> Artists { get; init; } = new();
    
    /// <summary>
    /// Artist of the album.
    /// </summary>
    [Obsolete("More than one artist might be the performers of an album. Use Artists property instead.")]
    [JsonPropertyName("artist")] public ArtistDetail? Artist { get; init; }
    
    /// <summary>
    /// Brief description of the album.
    /// </summary>
    [JsonPropertyName("briefDesc")] public string? BriefDescription { get; init; }
    
    /// <summary>
    /// Unix timestamp of the album publish date.
    /// </summary>
    [JsonPropertyName("publishTime")] public long PublishTimestamp { get; init; }
    
    /// <summary>
    /// Company that the album's copyright belongs to.
    /// </summary>
    [JsonPropertyName("company")] public string? Company { get; init; }
    
    /// <summary>
    /// Url of the album's artwork image.
    /// </summary>
    [JsonPropertyName("picUrl")] public string? ArtworkImageUrl { get; init; }
    
    /// <summary>
    /// Url of the album's blurred artwork image.
    /// </summary>
    [JsonPropertyName("blurPicUrl")] public string? BlurredArtworkImageUrl { get; init; }
    
    /// <summary>
    /// Description of the album.
    /// </summary>
    [JsonPropertyName("description")] public string? Description { get; init; }
    
    /// <summary>
    /// Tags of the album.
    /// </summary>
    // TODO: Determine string or List{string}.
    [JsonPropertyName("tags")] public string? Tags { get; init; }
    
    /// <summary>
    /// Status of the album, defined by NetEase.
    /// </summary>
    [JsonPropertyName("status")] public int Status { get; init; }
    
    /// <summary>
    /// Subtype of the album.
    /// </summary>
    [JsonPropertyName("subType")] public string? AlbumSubtype { get; init; }
    
    /// <summary>
    /// Title of the album.
    /// </summary>
    [JsonPropertyName("name")] public string Title { get; init; } = "Unknown Album";
    
    /// <summary>
    /// ID of the album, defined by NetEase.
    /// </summary>
    [JsonPropertyName("id")] public ulong AlbumId { get; init; }
    
    /// <summary>
    /// Type of the album.
    /// </summary>
    [JsonPropertyName("type")] public string? AlbumType { get; init; }
    
    /// <summary>
    /// Total count of tracks on the album.
    /// </summary>
    [JsonPropertyName("size")] public int TrackCount { get; init; }
    
    /// <summary>
    /// Whether the current user subscribes to the current album.
    /// </summary>
    [JsonPropertyName("isSub")] public bool IsSubscribed { get; init; }
    
    public OnlineAlbum ToBusinessModel() => new()
    {
        AlbumId = AlbumId,
        Title = Title,
        IsPurchased = IsPurchased,
        IsExplicit = (SpecialMarks & 0xFFFFFF) == 0x102000,
        IsOnSale = IsOnSale,
        Alias = Alias.ToArray(),
        Artists = Artists.ToArray(),
        PublishTime = DateTimeOffset.FromUnixTimeMilliseconds(PublishTimestamp),
        Company = Company,
        ArtworkImageUrl = ArtworkImageUrl,
        Description = Description,
        AlbumType = AlbumType,
        AlbumSubtype = AlbumSubtype,
        TrackCount = TrackCount,
        IsSubscribed = IsSubscribed
    };
}

/// <summary>
/// Detail of an album.
/// </summary>
[DebuggerDisplay("Title: {Title}, ID: {AlbumId}")]
public class AlbumDetail2 : IAlbum
{
    /// <summary>
    /// Unix timestamp that the album was subscribed.
    /// </summary>
    [JsonPropertyName("subTime")] public long SubscribedTimestamp { get; init; }
    
    /// <summary>
    /// Alias of the album title.
    /// </summary>
    [JsonPropertyName("alias")] public List<string> Alias { get; init; } = new();
    
    /// <summary>
    /// Artists of the album.
    /// </summary>
    [JsonPropertyName("artists")] public List<ArtistDetail> Artists { get; init; } = new();
    
    /// <summary>
    /// Url of the album's artwork image.
    /// </summary>
    [JsonPropertyName("picUrl")] public string? ArtworkImageUrl { get; init; }
    
    /// <summary>
    /// Title of the album.
    /// </summary>
    [JsonPropertyName("name")] public string Title { get; init; } = "Unknown Album";
    
    /// <summary>
    /// ID of the album, defined by NetEase.
    /// </summary>
    [JsonPropertyName("id")] public ulong AlbumId { get; init; }
    
    /// <summary>
    /// Total count of tracks on the album.
    /// </summary>
    [JsonPropertyName("size")] public int TrackCount { get; init; }
    
    /// <summary>
    /// Translations for the album's title.
    /// </summary>
    [JsonPropertyName("transNames")] public List<string> TitleTranslations { get; init; } = new();
}