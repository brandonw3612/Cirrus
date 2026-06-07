using System.Diagnostics;
using System.Text.Json.Serialization;
using Cirrus.Models.Business.Track;
using Cirrus.Models.Network.Album;
using Cirrus.Models.Network.Artist;
using Cirrus.Models.Shared.Album;
using Cirrus.Models.Shared.Artist;
using TrackPermission = Cirrus.Models.Network.Permission.TrackPermission;

namespace Cirrus.Models.Network.Track;

/// <summary>
/// Detail of a track.
/// </summary>
[DebuggerDisplay("Title: {Title}, ID: {TrackId}")]
public class TrackDetail
{
    /// <summary>
    /// Title of the track.
    /// </summary>
    [JsonPropertyName("name")] public string Title { get; init; } = "Unknown Track";
    
    /// <summary>
    /// ID of the track, defined by NetEase.
    /// </summary>
    [JsonPropertyName("id")] public ulong TrackId { get; init; }
    
    /// <summary>
    /// Track type related to its cloud properties.
    /// </summary>
    /// <remarks>
    /// = 1, if the track was uploaded to cloud by the user,
    /// yet cannot be matched with any existing tracks in the library; <br/>
    /// = 2, if the track was uploaded to cloud by the user,
    /// and a track from the library is matched with the track; <br/>
    /// = 0, otherwise.
    /// </remarks>
    [JsonPropertyName("t")] public int CloudType { get; init; }

    /// <summary>
    /// Artists of the track.
    /// </summary>
    [JsonPropertyName("ar")] public List<TrackArtist>? Artists { get; init; } = new();

    /// <summary>
    /// Title alias of the track.
    /// </summary>
    [JsonPropertyName("alia")] public List<string>? Alias { get; init; } = new();
    
    /// <summary>
    /// Popularity of the track. Range from 0 to 100.
    /// </summary>
    [JsonPropertyName("pop")] public double Popularity { get; init; }
    
    /// <summary>
    /// Charging-type of the track for different members.
    /// </summary>
    /// <remarks>
    /// = 0, available for any users; <br/>
    /// = 1, available for VIP users only; <br/>
    /// = 4, from a digital album that is sold separately; <br/>
    /// = 8, available for any users (normal quality only). 
    /// </remarks>
    [JsonPropertyName("fee")] public int ChargingType { get; init; }
    
    /// <summary>
    /// Album of the track.
    /// </summary>
    [JsonPropertyName("al")] public TrackAlbum? Album { get; init; }
    
    /// <summary>
    /// Duration of the track, presented in total milliseconds.
    /// </summary>
    [JsonPropertyName("dt")] public long DurationMilliseconds { get; init; }
    
    /// <summary>
    /// Information for the high quality audio of the track.
    /// </summary>
    [JsonPropertyName("h")] public TrackAudioInfo? HighQualityAudioInfo { get; init; }
    
    /// <summary>
    /// Information for the medium quality audio of the track.
    /// </summary>
    [JsonPropertyName("m")] public TrackAudioInfo? MediumQualityAudioInfo { get; init; }
    
    /// <summary>
    /// Information for the low quality audio of the track.
    /// </summary>
    [JsonPropertyName("l")] public TrackAudioInfo? LowQualityAudioInfo { get; init; }
    
    /// <summary>
    /// Title of the disc that current track is on.
    /// </summary>
    [JsonPropertyName("cd")] public string? DiscTitle { get; init; }
    
    /// <summary>
    /// Number of the track in the disc. 0 by default.
    /// </summary>
    [JsonPropertyName("no")] public int NumberInDisc { get; init; }
    
    /// <summary>
    /// ID of the podcast's creator. 0 if the track is not a podcast.
    /// </summary>
    [JsonPropertyName("djId")] public ulong CreatorId { get; init; }
    
    /// <summary>
    /// ID of the track matched with current track, if current track was uploaded to the cloud by the user.
    /// </summary>
    [JsonPropertyName("s_id")] public ulong MatchedTrackId { get; init; }
    
    /// <summary>
    /// Special marks of the track. 
    /// </summary>
    /// <remarks>
    /// = 0x142000 | 0x140000, the track is marked as "Explicit".
    /// </remarks>
    [JsonPropertyName("mark")] public long SpecialMarks { get; init; }
    
    /// <summary>
    /// A value indicating whether the track is an original or a cover.
    /// </summary>
    /// <remarks>
    /// = 0 | 1, original; <br/>
    /// = 2, cover.
    /// </remarks>
    // TODO: Further insight needed on the difference between 0 and 1.
    [JsonPropertyName("originCoverType")] public int OriginCoverType { get; init; }
    
    /// <summary>
    /// A value indicating whether the track's album is unavailable.
    /// </summary>
    /// <remarks>
    /// 0, the album is available; <br/>
    /// 1, the album is unavailable.
    /// </remarks>
    [JsonPropertyName("single")] public int AlbumUnavailability { get; init; }
    
    /// <summary>
    /// ID of the track's music video.
    /// </summary>
    [JsonPropertyName("mv")] public ulong MusicVideoId { get; init; }
    
    /// <summary>
    /// Unix timestamp of the track's publish time, in milliseconds.
    /// </summary>
    [JsonPropertyName("publishTime")] public long PublishTimestamp { get; init; }
    
    /// <summary>
    /// Permission for the track.
    /// </summary>
    [JsonInclude] [JsonPropertyName("privilege")] public TrackPermission? Permission { get; internal set; }
    
    /// <summary>
    /// Original track information, only when current track is a cover.
    /// </summary>
    [JsonPropertyName("originSongSimpleData")] public OriginalTrackInfo? OriginalTrack { get; init; }

    /// <summary>
    /// Translations for the track's title.
    /// </summary>
    [JsonPropertyName("tns")] public List<string>? TitleTranslations { get; init; } = new();

    public OnlineTrack ToBusinessModel() => new()
    {
        TrackId = TrackId,
        Title = Title,
        Alias = Alias?.ToArray() ?? [],
        TitleTranslations = TitleTranslations?.ToArray() ?? [],
        IsExplicit = (SpecialMarks & 0xFF0000) == 0x140000,
        Artists = Artists?.ToArray() ?? [],
        Album = Album,
        PublishDate = DateTimeOffset.FromUnixTimeMilliseconds(PublishTimestamp),
        DiscNumber = int.TryParse(DiscTitle, out var number) ? number : -1,
        NumberInDisc = NumberInDisc,
        Popularity = Popularity,
        Permission = new()
        {
            CopyrightUnavailable = Permission is { TrackAvailability: -200 },
            IsFromCloudDrive = Permission is { IsFromCloudDrive: true },
            IsMembershipRequired = ChargingType is 1,
            IsPurchased = Permission is { PurchaseStatus: 3 or 5 },
            IsPurchaseRequired = ChargingType is 4
        },
        AvailableQualities = ((TrackAudioInfo?[]) [LowQualityAudioInfo, MediumQualityAudioInfo, HighQualityAudioInfo])
            .OfType<TrackAudioInfo>().Select(static ai => ai.BitRate).ToArray(),
        Duration = TimeSpan.FromMilliseconds(DurationMilliseconds)
    };
}

/// <summary>
/// Detail of a track.
/// </summary>
[DebuggerDisplay("Title: {Title}, ID: {TrackId}")]
public class TrackDetail2
{
    /// <summary>
    /// Title of the track.
    /// </summary>
    [JsonPropertyName("name")] public string Title { get; init; } = "Unknown Track";
    
    /// <summary>
    /// ID of the track, defined by NetEase.
    /// </summary>
    [JsonPropertyName("id")] public ulong TrackId { get; init; }
    
    /// <summary>
    /// Title alias of the track.
    /// </summary>
    [JsonPropertyName("alias")] public List<string>? Alias { get; init; } = new();
    
    /// <summary>
    /// Charging-type of the track for different members.
    /// </summary>
    /// <remarks>
    /// = 0, available for any users; <br/>
    /// = 1, available for VIP users only; <br/>
    /// = 4, from a digital album that is sold separately; <br/>
    /// = 8, available for any users (normal quality only). 
    /// </remarks>
    [JsonPropertyName("fee")] public int ChargingType { get; init; }
    
    /// <summary>
    /// Artists of the track.
    /// </summary>
    [JsonPropertyName("artists")] public List<ArtistDetail>? Artists { get; init; } = new();
    
    /// <summary>
    /// Album of the track.
    /// </summary>
    [JsonPropertyName("album")] public AlbumDetail? Album { get; init; }
    
    /// <summary>
    /// Duration of the track, presented in total milliseconds.
    /// </summary>
    [JsonPropertyName("duration")] public long DurationMilliseconds { get; init; }
    
    /// <summary>
    /// ID of the track's music video.
    /// </summary>
    [JsonPropertyName("mvid")] public ulong MusicVideoId { get; init; }

    public OnlineTrack ToBusinessModel() => new()
    {
        TrackId = TrackId,
        Title = Title,
        Alias = Alias?.ToArray() ?? [],
        TitleTranslations = [],
        IsExplicit = false,
        Artists = Artists?.Select(a => new TrackArtist
        {
            ArtistId = a.ArtistId,
            Name = a.Name,
            NameTranslations = a.NameTranslation is { Length: > 0 } ? [a.NameTranslation] : [],
            Alias = a.Alias
        }).ToArray() ?? [],
        Album = Album is { } album ? new TrackAlbum
        {
            AlbumId = album.AlbumId,
            Title = album.Title,
            ArtworkImageUrl = album.ArtworkImageUrl,
            TitleTranslations = []
        } : null,
        PublishDate = DateTimeOffset.FromUnixTimeMilliseconds(0),
        DiscNumber = -1,
        NumberInDisc = -1,
        Popularity = 0,
        Permission = new()
        {
            CopyrightUnavailable = false,
            IsFromCloudDrive = false,
            IsMembershipRequired = ChargingType is 1,
            IsPurchased = false,
            IsPurchaseRequired = ChargingType is 4
        },
        AvailableQualities = [],
        Duration = TimeSpan.FromMilliseconds(DurationMilliseconds)
    };
}

/// <summary>
/// Detail of a track.
/// </summary>
[DebuggerDisplay("Title: {Title}, ID: {TrackId}")]
public class TrackDetail3
{
    /// <summary>
    /// Title of the track.
    /// </summary>
    [JsonPropertyName("name")] public string Title { get; init; } = "Unknown Track";
    
    /// <summary>
    /// ID of the track, defined by NetEase.
    /// </summary>
    [JsonPropertyName("id")] public ulong TrackId { get; init; }
    
    /// <summary>
    /// Track type related to its cloud properties.
    /// </summary>
    /// <remarks>
    /// = 1, if the track was uploaded to cloud by the user,
    /// yet cannot be matched with any existing tracks in the library; <br/>
    /// = 2, if the track was uploaded to cloud by the user,
    /// and a track from the library is matched with the track; <br/>
    /// = 0, otherwise.
    /// </remarks>
    [JsonPropertyName("t")] public int CloudType { get; init; }

    /// <summary>
    /// Artists of the track.
    /// </summary>
    [JsonPropertyName("artists")] public List<ArtistDetail> Artists { get; init; } = new();

    /// <summary>
    /// Title alias of the track.
    /// </summary>
    [JsonPropertyName("alias")] public List<string> Alias { get; init; } = new();
    
    /// <summary>
    /// Popularity of the track. Range from 0 to 100.
    /// </summary>
    [JsonPropertyName("popularity")] public double Popularity { get; init; }
    
    /// <summary>
    /// Charging-type of the track for different members.
    /// </summary>
    /// <remarks>
    /// = 0, available for any users; <br/>
    /// = 1, available for VIP users only; <br/>
    /// = 4, from a digital album that is sold separately; <br/>
    /// = 8, available for any users (normal quality only). 
    /// </remarks>
    [JsonPropertyName("fee")] public int ChargingType { get; init; }
    
    /// <summary>
    /// Album of the track.
    /// </summary>
    [JsonPropertyName("album")] public AlbumDetail? Album { get; init; }
    
    /// <summary>
    /// Duration of the track, presented in total milliseconds.
    /// </summary>
    [JsonPropertyName("duration")] public long DurationMilliseconds { get; init; }
    
    /// <summary>
    /// Information for the high quality audio of the track.
    /// </summary>
    [JsonPropertyName("hMusic")] public TrackAudioInfo2? HighQualityAudioInfo { get; init; }
    
    /// <summary>
    /// Information for the medium quality audio of the track.
    /// </summary>
    [JsonPropertyName("mMusic")] public TrackAudioInfo2? MediumQualityAudioInfo { get; init; }
    
    /// <summary>
    /// Information for the low quality audio of the track.
    /// </summary>
    [JsonPropertyName("lMusic")] public TrackAudioInfo2? LowQualityAudioInfo { get; init; }
    
    /// <summary>
    /// Title of the disc that current track is on.
    /// </summary>
    [JsonPropertyName("disc")] public string? DiscTitle { get; init; }
    
    /// <summary>
    /// Number of the track in the disc. 0 by default.
    /// </summary>
    [JsonPropertyName("no")] public int NumberInDisc { get; init; }
    
    /// <summary>
    /// ID of the podcast's creator. 0 if the track is not a podcast.
    /// </summary>
    [JsonPropertyName("djId")] public ulong CreatorId { get; init; }
    
    /// <summary>
    /// ID of the track matched with current track, if current track was uploaded to the cloud by the user.
    /// </summary>
    [JsonPropertyName("s_id")] public ulong MatchedTrackId { get; init; }
    
    /// <summary>
    /// Special marks of the track. 
    /// </summary>
    /// <remarks>
    /// = 0x142000 | 0x140000, the track is marked as "Explicit".
    /// </remarks>
    [JsonPropertyName("mark")] public long SpecialMarks { get; init; }
    
    /// <summary>
    /// A value indicating whether the track is an original or a cover.
    /// </summary>
    /// <remarks>
    /// = 0 | 1, original; <br/>
    /// = 2, cover.
    /// </remarks>
    // TODO: Further insight needed on the difference between 0 and 1.
    [JsonPropertyName("originCoverType")] public int OriginCoverType { get; init; }
    
    /// <summary>
    /// A value indicating whether the track's album is unavailable.
    /// </summary>
    /// <remarks>
    /// 0, the album is available; <br/>
    /// 1, the album is unavailable.
    /// </remarks>
    [JsonPropertyName("single")] public int AlbumUnavailability { get; init; }
    
    /// <summary>
    /// ID of the track's music video.
    /// </summary>
    [JsonPropertyName("mv")] public ulong MusicVideoId { get; init; }
    
    /// <summary>
    /// Unix timestamp of the track's publish time, in milliseconds.
    /// </summary>
    [JsonPropertyName("publishTime")] public long PublishTimestamp { get; init; }
    
    /// <summary>
    /// Permission for the track.
    /// </summary>
    [JsonPropertyName("privilege")] public TrackPermission? Permission { get; init; }
    
    /// <summary>
    /// Original track information, only when current track is a cover.
    /// </summary>
    [JsonPropertyName("originSongSimpleData")] public OriginalTrackInfo? OriginalTrack { get; init; }

    /// <summary>
    /// Translations for the track's title.
    /// </summary>
    [JsonPropertyName("transNames")] public List<string>? TitleTranslations { get; init; } = new();

    public OnlineTrack ToBusinessModel() => new()
    {
        TrackId = TrackId,
        Title = Title,
        Alias = Alias.ToArray(),
        TitleTranslations = TitleTranslations?.ToArray() ?? [],
        IsExplicit = (SpecialMarks & 0xFF0000) == 0x140000,
        Artists = Artists.Select(static a => new TrackArtist
        {
            ArtistId = a.ArtistId,
            Name = a.Name,
            Alias = a.Alias,
            NameTranslations = a.NameTranslation is not null ? [a.NameTranslation] : []
        }).ToArray(),
        Album = Album is not null ? new()
        {
            AlbumId = Album.AlbumId,
            ArtworkImageUrl = Album.ArtworkImageUrl,
            Title = Album.Title
        } : null,
        PublishDate = DateTimeOffset.FromUnixTimeMilliseconds(PublishTimestamp),
        DiscNumber = int.TryParse(DiscTitle, out var number) ? number : -1,
        NumberInDisc = NumberInDisc,
        Popularity = Popularity,
        Permission = new()
        {
            CopyrightUnavailable = Permission is { TrackAvailability: -200 },
            IsFromCloudDrive = Permission is { IsFromCloudDrive: true },
            IsMembershipRequired = ChargingType is 1,
            IsPurchased = Permission is { PurchaseStatus: 3 or 5 },
            IsPurchaseRequired = ChargingType is 4
        },
        AvailableQualities = ((TrackAudioInfo2?[]) [LowQualityAudioInfo, MediumQualityAudioInfo, HighQualityAudioInfo])
            .OfType<TrackAudioInfo2>().Select(static ai => ai.BitRate).ToArray(),
        Duration = TimeSpan.FromMilliseconds(DurationMilliseconds)
    };
}