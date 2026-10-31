using Cirrus.Models.Network.Artist;

namespace Cirrus.Models.Business.Album;

public class OnlineAlbumBrief
{
    public ulong AlbumId { get; init; }
    public string Title { get; init; } = "Unknown Album";
    public string[] Alias { get; init; } = [];
    public string[] TitleTranslations { get; init; } = [];
    public ArtistDetail[] Artists { get; init; } = [];
    public int TrackCount { get; init; }
    public string? ArtworkImageUrl { get; init; }
}