namespace Cirrus.Data;

public static class TTL
{
    public static readonly TimeSpan Track = TimeSpan.FromDays(7);
    public static readonly TimeSpan Lyrics = TimeSpan.FromDays(30);
    public static readonly TimeSpan Playlist = TimeSpan.FromDays(1);

    public static class Personal
    {
        public static readonly TimeSpan Profile = TimeSpan.FromDays(14);
        public static readonly TimeSpan Playlists = TimeSpan.FromDays(1);
    }
}