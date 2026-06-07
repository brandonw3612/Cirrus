namespace Cirrus.Models.Business.Lyrics;

public class LyricFragment  :ITimestamp
{
    public TimeSpan Start { get; set; }
    public TimeSpan Duration { get; set; }
    public string Text { get; set; } = string.Empty;
    public override string ToString() => Text;
}