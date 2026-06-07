using Cirrus.Models.Abstract.Primitives;

namespace Cirrus.Models.Business.Developer;

public interface ISoftwareUpdate : IComparable<ISoftwareUpdate>, INavigatiable
{
    string ShortVersion { get; }
    string LongVersion { get; }
    bool IsUpdatable { get; }
    string ReleaseNotes { get; }
    string DownloadUrl { get; }
    ISet<string> Channels { get; }
    DateTimeOffset PublishedTime { get; }
    bool IsEnabled { get; }
    public string Architecture { get; }

    public string VersionHeader => $"{ShortVersion} ({PublishedTime:d})";
    public bool IsPublic => Channels.Contains("Public");
    public bool IsInPreview => Channels.Contains("Preview");
}