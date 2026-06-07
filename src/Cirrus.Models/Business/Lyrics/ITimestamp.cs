namespace Cirrus.Models.Business.Lyrics;

public interface ITimestamp
{
    TimeSpan Start { get; }
    TimeSpan Duration { get; }
}