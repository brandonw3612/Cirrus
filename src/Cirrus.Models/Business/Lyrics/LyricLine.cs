namespace Cirrus.Models.Business.Lyrics;

public class LyricLine : ITimestamp, IComparable<LyricLine>
{
    public TimeSpan Start { get; set; }
    public TimeSpan Duration { get; set; }
    public List<LyricFragment> B2BLyrics { get; set; } = [];
    public string FallbackText { get; set; } = string.Empty;
    public string Translation { get; set; } = string.Empty;

    public override string ToString() => FallbackText;

    public int CompareTo(LyricLine? other)
    {
        if (other is null) return 1;
        if (Start != other.Start) return Start.CompareTo(other.Start);
        return FallbackText != other.FallbackText
            ? string.Compare(FallbackText, other.FallbackText, StringComparison.Ordinal)
            : 0;
    }
}