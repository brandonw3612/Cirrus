using Cirrus.Models.Abstract;
using Cirrus.Models.Business.Playback;
using Cirrus.Models.Shared.Album;
using Cirrus.Models.Shared.Artist;

namespace Cirrus.Models.Business.Track;

public class OnlineTrack : IAudioTrack<ulong>, ITrack
{
    public required ulong TrackId { get; init; }
    public required string Title { get; init; }
    public required string[] Alias { get; init; }
    public required string[] TitleTranslations { get; init; }
    public required bool IsExplicit { get; init; }
    public required TrackArtist[] Artists { get; init; }
    public required TrackAlbum? Album { get; init; }
    public required DateTimeOffset PublishDate { get; init; }
    public required int DiscNumber { get; init; }
    public required int NumberInDisc { get; init; }
    public required double Popularity { get; init; }
    public required TrackPermission Permission { get; init; }
    public required int[] AvailableQualities { get; init; }

    public string DisplayArtist => Artists.Length switch
    {
        0 => "Unknown Artist",
        1 => Artists[0].Name,
        _ => string.Join(", ", Artists[..^1].Select(static a => a.Name)) + " & " + Artists[^1].Name
    };

    public string DisplayAlbum => Album?.Title ?? "Unknown Album";
    public required TimeSpan Duration { get; init; }
    public Uri? AlbumArtworkUri => Album is { ArtworkImageUrl: { } url } ? new(url) : null;
}