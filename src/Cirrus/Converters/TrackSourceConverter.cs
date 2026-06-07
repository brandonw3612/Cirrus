using Cirrus.Models.Business.Track;
using Microsoft.UI.Xaml.Data;

namespace Cirrus.Converters;

public partial class TrackSourceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not OnlineTrack track) return string.Empty;
        var artistNames = track.Artists.Select(static t => t.Name).ToArray();
        var mergedArtists = artistNames.Length switch
        {
            0 => "Unknown Artist",
            1 => artistNames.First(),
            _ => string.Join(", ", artistNames[..^1]) + " & " + artistNames[^1]
        };
        var albumTitle = track.Album?.Title ?? "Unknown Album";
        return $"{mergedArtists} · {albumTitle}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}