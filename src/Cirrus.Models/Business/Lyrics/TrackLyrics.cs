using Cirrus.Models.Network.User;

namespace Cirrus.Models.Business.Lyrics;

public class TrackLyrics
{
    public List<LyricLine> Lines { get; set; } = [];
    public LyricsContributor? SyncedLyricsContributor { get; set; }
    public LyricsContributor? TranslationContributor { get; set; }
    public object? AdditionalContent { get; set; }
    public string FallbackAdditionalContent { get; set; } = string.Empty;
}