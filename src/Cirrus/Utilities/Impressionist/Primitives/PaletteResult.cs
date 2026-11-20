using Wacton.Unicolour;

namespace Cirrus.Utilities.Impressionist.Primitives
{
    public class PaletteResult(List<Unicolour> palette, Unicolour themeColor)
    {
        public List<Unicolour> Palette { get; } = palette;
        public Unicolour ThemeColor { get; } = themeColor;
    }
}
