using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Cirrus.Utilities;

public class LocalImageAssetMaintainer
{
    private static LocalImageAssetMaintainer? _instance;
    public static LocalImageAssetMaintainer Instance => _instance ??= new();

    private LocalImageAssetMaintainer()
    {
        // Hidden constructor.
    }
    
    private readonly Dictionary<string, ImageSource> _imageSources = new();
    
    public ImageSource? GetImageSource(string path)
    {
        if (_imageSources.TryGetValue(path, out var imageSource)) return imageSource;
        if (!path.StartsWith("ms-appx:///")) return null;
        if (path.EndsWith(".svg"))
        {
            var svgImage = new SvgImageSource(new(path));
            _imageSources.Add(path, svgImage);
            return svgImage;
        }
        var bitmapImage = new BitmapImage(new(path));
        _imageSources.Add(path, bitmapImage);
        return bitmapImage;
    }
}