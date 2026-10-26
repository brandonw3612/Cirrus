using Cirrus.Models.Business.Track;

namespace Cirrus.LiveModels;

public record IndexedOnlineTrack
{
    public required OnlineTrack Track { get; init; }
    public string Index { get; init; } = string.Empty;
    public double Percentage { get; init; } = 0d;
    public string Plays { get; init; } = string.Empty;
}

public partial record IndexedOnlineAlbumTrack : IndexedOnlineTrack
{
    public required bool AreArtistsShown { get; init; }
}