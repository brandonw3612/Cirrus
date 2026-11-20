using Windows.Graphics.Imaging;
using Wacton.Unicolour;

namespace Cirrus.Extensions
{
    public static class BitmapExtensions
    {
        public static async Task<Dictionary<Unicolour, int>> GetColorMap(this BitmapDecoder bitmapDecoder)
        {
            var pixelDataProvider = await bitmapDecoder.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Straight,
                new BitmapTransform(),
                ExifOrientationMode.IgnoreExifOrientation,
                ColorManagementMode.ColorManageToSRgb
            );
            var pixels = pixelDataProvider.DetachPixelData();
            var count = bitmapDecoder.PixelWidth * bitmapDecoder.PixelHeight;
            var vector = new Dictionary<Unicolour, int>();
            for (int i = 0; i < count; i += 10)
            {
                var offset = i * 4;
                var b = pixels[offset];
                var g = pixels[offset + 1];
                var r = pixels[offset + 2];
                var a = pixels[offset + 3];
                if (a == 0) continue;
                Unicolour color = new(ColourSpace.Rgb255, r, g, b);
                vector[color] = vector.GetValueOrDefault(color, 0) + 1;
            }
            return vector;
        }
    }
}