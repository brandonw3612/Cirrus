using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cirrus.Models.Abstract;
using Cirrus.Models.Business.Playlist;
using Cirrus.Models.Network.Track;
using Cirrus.Models.Network.User;

namespace Cirrus.Models.Network.Playlist;

/// <summary>
/// Detail of a playlist.
/// </summary>
[DebuggerDisplay("Title: {Title}, ID: {PlaylistId}")]
public class PlaylistDetail : IPlaylist, IJsonOnDeserialized
{
    /// <summary>
    /// Whether the playlist is subscribed by the current user.
    /// </summary>
    [JsonPropertyName("subscribed")] public bool? IsSubscribed { get; init; }
    
    /// <summary>
    /// Creator / Owner of the playlist.
    /// </summary>
    [JsonPropertyName("creator")] public UserProfile? Creator { get; init; }
    
    /// <summary>
    /// Total count of users subscribing to the playlist.
    /// </summary>
    [JsonPropertyName("subscribedCount")] public int SubscriberCount { get; init; }

    /// <summary>
    /// List of users subscribing to the playlist.
    /// </summary>
    [JsonPropertyName("subscribers")] public List<UserProfile> Subscribers { get; init; } = new();
    
    /// <summary>
    /// Total count of cloud tracks in the playlist.
    /// </summary>
    [JsonPropertyName("cloudTrackCount")] public int CloudTrackCount { get; init; }
    
    /// <summary>
    /// ID of the playlist creator / owner.
    /// </summary>
    [JsonPropertyName("userId")] public ulong CreatorId { get; init; }

    /// <summary>
    /// Tracks on the playlist, usually not complete.
    /// </summary>
    [Obsolete("Usually the list is not complete. Take the TrackIds and request for the complete list instead.")]
    [JsonPropertyName("tracks")] public List<TrackDetail> Tracks { get; init; } = new();

    /// <summary>
    /// Complete list of ID of the tracks on the playlist.
    /// </summary>
    [JsonIgnore] public List<ulong> TrackIds { get; private set; } = new();
    
    /// <summary>
    /// Privacy type of the playlist, presented in an integer.
    /// </summary>
    /// <remarks>
    /// 10 - Private playlist.
    /// </remarks>
    [JsonPropertyName("privacy")] public int PrivacyType { get; init; }
    
    /// <summary>
    /// Unix timestamp of the last update on the tracks in the playlist.
    /// </summary>
    [JsonPropertyName("trackUpdateTime")] public long TrackUpdatedTimestamp { get; init; }
    
    /// <summary>
    /// Total count of tracks in the playlist.
    /// </summary>
    [JsonPropertyName("trackCount")] public int TrackCount { get; init; }
    
    /// <summary>
    /// Unix timestamp of the last update on the playlist.
    /// </summary>
    [JsonPropertyName("updateTime")] public long UpdatedTimestamp { get; init; }
    
    /// <summary>
    /// Url of the playlist's cover image.
    /// </summary>
    [JsonPropertyName("coverImgUrl")] public string? CoverImageUrl { get; init; }
    
    /// <summary>
    /// Type of the playlist, presented in an integer.
    /// </summary>
    /// <remarks>
    /// 5 - User like list;
    /// 10 - Top chart;
    /// 100 - Official dynamic playlist;
    /// 20 - User's annual top chart;
    /// 300 - Shared playlist;
    /// Others - Normal Playlist.
    /// </remarks>
    [JsonPropertyName("specialType")] public int PlaylistType { get; init; }
    
    /// <summary>
    /// Unix timestamp that the playlist was created.
    /// </summary>
    [JsonPropertyName("createTime")] public long CreatedTimestamp { get; init; }
    
    /// <summary>
    /// Total plays of the playlist since created.
    /// </summary>
    [JsonPropertyName("playCount")] public long Plays { get; init; }
    
    /// <summary>
    /// Description of the playlist.
    /// </summary>
    [JsonPropertyName("description")] public string? Description { get; init; }
    
    /// <summary>
    /// Tags of the playlist.
    /// </summary>
    [JsonPropertyName("tags")] public List<string> Tags { get; init; } = new();
    
    /// <summary>
    /// Status of the playlist.
    /// </summary>
    // TODO: Classification / Documentation needed.
    [JsonPropertyName("status")] public int Status { get; init; }
    
    /// <summary>
    /// Title of the playlist.
    /// </summary>
    [JsonPropertyName("name")] public string Title { get; set; } = "Unknown Playlist";
    
    /// <summary>
    /// ID of the playlist, defined by NetEase.
    /// </summary>
    [JsonPropertyName("id")] public ulong PlaylistId { get; init; }
    
    /// <summary>
    /// Update frequency for official dynamic playlists.
    /// </summary>
    [JsonPropertyName("updateFrequency")] public string? UpdateFrequency { get; init; }
    
    /// <summary>
    /// Url of the playlist's background cover image.
    /// </summary>
    [JsonPropertyName("backgroundCoverUrl")] public string? BackgroundCoverImageUrl { get; init; }
    
    /// <summary>
    /// Url of the playlist's title image.
    /// </summary>
    [JsonPropertyName("titleImageUrl")] public string? TitleImageUrl { get; init; }
    
    /// <summary>
    /// English title of the playlist, given by NetEase.
    /// </summary>
    [JsonPropertyName("englishTitle")] public string? EnglishTitle { get; init; }
    
    /// <summary>
    /// Shared creators of the playlist.
    /// </summary>
    [JsonPropertyName("sharedUsers")] public List<UserProfile>? SharedCreators { get; init; } = new();

    [JsonInclude] [JsonExtensionData] internal Dictionary<string, JsonElement> ExtensionData { get; set; } = new();

    void IJsonOnDeserialized.OnDeserialized()
    {
        if (!ExtensionData.TryGetValue("trackIds", out var trackIds)
            || trackIds.ValueKind is not JsonValueKind.Array) return;
        List<ulong> tIds = [];
        foreach (var trackId in trackIds.EnumerateArray())
        {
            if (trackId.TryGetProperty("id", out var idElem)
                && idElem.TryGetUInt64(out var id))
            {
                tIds.Add(id);
            }
        }
        TrackIds = tIds;
        ExtensionData.Clear();
    }

    public OnlinePlaylist ToBusinessModel() => new()
    {
        BackgroundCoverImageUrl = BackgroundCoverImageUrl,
        CloudTrackCount = CloudTrackCount,
        CoverImageUrl = CoverImageUrl,
        CreatedTime = DateTimeOffset.FromUnixTimeMilliseconds(CreatedTimestamp),
        Creator = Creator,
        CreatorId = CreatorId,
        Description = Description,
        EnglishTitle = EnglishTitle,
        IsPrivate = PrivacyType == 10,
        IsSubscribed = IsSubscribed == true,
        PlaylistId = PlaylistId,
        Plays = Plays,
        SharedCreators = SharedCreators ?? [],
        SubscriberCount = SubscriberCount,
        Subscribers = Subscribers,
        Tags = Tags,
        Title = Title,
        TitleImageUrl = TitleImageUrl,
        TrackCount = TrackCount,
        TrackIds = TrackIds,
        TrackUpdatedTime = DateTimeOffset.FromUnixTimeMilliseconds(TrackUpdatedTimestamp),
        Type = PlaylistType switch
        {
            5 => Business.Playlist.PlaylistType.Likelist,
            10 => Business.Playlist.PlaylistType.Chart,
            100 => Business.Playlist.PlaylistType.Mix,
            20 => Business.Playlist.PlaylistType.Annual,
            300 => Business.Playlist.PlaylistType.Shared,
            _ => Business.Playlist.PlaylistType.Normal
        },
        UpdateFrequency = UpdateFrequency,
        UpdatedTime = DateTimeOffset.FromUnixTimeMilliseconds(UpdatedTimestamp)
    };
}