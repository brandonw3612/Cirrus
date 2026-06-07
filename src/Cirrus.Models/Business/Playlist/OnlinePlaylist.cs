using Cirrus.Models.Abstract;
using Cirrus.Models.Network.User;

namespace Cirrus.Models.Business.Playlist;

public class OnlinePlaylist : IPlaylist
{
    public required string? BackgroundCoverImageUrl { get; init; }
    public required int CloudTrackCount { get; init; }
    public required string? CoverImageUrl { get; init; }
    public required DateTimeOffset CreatedTime { get; set; }
    public required UserProfile? Creator { get; init; }
    public required ulong CreatorId { get; init; }
    public required string? Description { get; init; }
    public required string? EnglishTitle { get; init; }
    public required bool IsPrivate { get; set; }
    public required bool IsSubscribed { get; set; }
    public required ulong PlaylistId { get; init; }
    public required long Plays { get; init; }
    public required List<UserProfile> SharedCreators { get; init; } = new();
    public required int SubscriberCount { get; init; }
    public required List<UserProfile> Subscribers { get; init; }
    public required List<string> Tags { get; init; }
    public required string Title { get; init; }
    public required string? TitleImageUrl { get; init; }
    public required int TrackCount { get; init; }
    public required List<ulong> TrackIds { get; init; }
    public required DateTimeOffset TrackUpdatedTime { get; init; }
    public required PlaylistType Type { get; init; }
    public required string? UpdateFrequency { get; init; }
    public required DateTimeOffset UpdatedTime { get; init; }

    public QuickAccessPlaylist ToQuickAccess() => new()
    {
        PlaylistId = PlaylistId,
        Title = Title,
        CoverImageUrl = CoverImageUrl
    };
}