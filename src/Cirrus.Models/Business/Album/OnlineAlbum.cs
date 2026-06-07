using Cirrus.Models.Abstract;
using Cirrus.Models.Network.Artist;

namespace Cirrus.Models.Business.Album;

public class OnlineAlbum : IAlbum
{
    public required ulong AlbumId { get; init; }
    public required string Title { get; init; }
    public required bool IsExplicit { get; init; }
    public required bool IsPurchased { get; init; }
    public required bool IsOnSale { get; init; }
    public required string[] Alias { get; init; }
    public required ArtistDetail[] Artists { get; init; }
    public required DateTimeOffset PublishTime { get; init; }
    public required string? Company { get; init; }
    public required string? ArtworkImageUrl { get; init; }
    public required string? Description { get; init; }
    public required string? AlbumType { get; init; }
    public required string? AlbumSubtype { get; init; }
    public required int TrackCount { get; init; }
    public required bool IsSubscribed { get; set; }
}