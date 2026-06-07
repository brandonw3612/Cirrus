using Cirrus.Models.Abstract;

namespace Cirrus.Models.Business.User;

public class TopChart : ITopChart
{
    public ulong UserId { get; set; }
    public bool IsAccessible { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public long PlayedTrackCount { get; set; }
}