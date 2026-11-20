using Wacton.Unicolour;

namespace Cirrus.Utilities.Impressionist.Primitives;

public interface IPaletteExtractorConfig;

public interface IPaletteExtractor<in TConfig>
{
    public static abstract IPaletteExtractor<TConfig> Default { get; }
    
    Unicolour ExtractThemeColor(Dictionary<Unicolour, int> sourceColorMap, TConfig? config);
    
    PaletteResult ExtractPalette(Dictionary<Unicolour, int> sourceColorMap, int colorCount, TConfig? config);
}