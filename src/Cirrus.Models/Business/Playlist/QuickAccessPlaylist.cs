using Cirrus.Models.Abstract;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Cirrus.Models.Business.Playlist;

public partial class QuickAccessPlaylist : ObservableObject, IPlaylist
{
    public required ulong PlaylistId { get; init; }
    [ObservableProperty] public partial string Title { get; set; } = string.Empty;
    [ObservableProperty] public partial string? CoverImageUrl { get; set; }
}

public class QuickAccessPlaylistEqualityComparer : IEqualityComparer<QuickAccessPlaylist>
{
    private static QuickAccessPlaylistEqualityComparer? _instance;
    public static QuickAccessPlaylistEqualityComparer Instance => _instance ??= new();
    
    public bool Equals(QuickAccessPlaylist? x, QuickAccessPlaylist? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.PlaylistId == y.PlaylistId;
    }

    public int GetHashCode(QuickAccessPlaylist obj)
    {
        return obj.PlaylistId.GetHashCode();
    }
}