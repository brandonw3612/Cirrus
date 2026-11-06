using Windows.UI;
using Cirrus.Composition;
using Cirrus.Primitives;
using Microsoft.UI.Composition;
using Microsoft.UI;

namespace Cirrus.Extensions;

public static class CompositionResourceExtensions
{
    public static ThemeAwareResource<AcrylicComposition> BuildDefaultThemeAwareAcrylicBrush(this Compositor compositor)
    {
        return new(
            new(compositor)
            {
                TintColor = Color.FromArgb(0x00, 0x00, 0x00, 0x00),
                TintOpacity = 1,
                TintLuminosityOpacity = 0.96,
                FallbackColor = Color.FromArgb(0xFF, 0x7F, 0x7F, 0x7F)
            }
        )
        {
            ToLight = c =>
            {
                c.TintColor = Color.FromArgb(0xFF, 0xFC, 0xFC, 0xFC);
                c.TintOpacity = 0f;
                c.FallbackColor = Color.FromArgb(0xFF, 0xF9, 0xF9, 0xF9);
                c.TintLuminosityOpacity = 0.85f;
            },
            ToDark = c =>
            {
                c.TintColor = Color.FromArgb(0xFF, 0x2C, 0x2C, 0x2C);
                c.TintOpacity = 0.15f;
                c.FallbackColor = Color.FromArgb(0xFF, 0x2C, 0x2C, 0x2C);
                c.TintLuminosityOpacity = 0.96f;
            }
        };
    }
    
    public static ThemeAwareResource<CompositionColorBrush> BuildThemeAwareCardBackgroundSolidBrush(this Compositor compositor)
    {
        return new(
            compositor.CreateColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF))
        )
        {
            ToLight = c => c.Color = Color.FromArgb(0xB3, 0xFF, 0xFF, 0xFF),
            ToDark = c => c.Color = Color.FromArgb(0x0D, 0xFF, 0xFF, 0xFF)
        };
    }
    
    public static ThemeAwareResource<CompositionColorBrush> BuildThemeAwareCardStrokeBrush(this Compositor compositor)
    {
        return new(
            compositor.CreateColorBrush(Color.FromArgb(0x00, 0x00, 0x00, 0x00))
        )
        {
            ToLight = c => c.Color = Color.FromArgb(0x0F, 0x00, 0x00, 0x00),
            ToDark = c => c.Color = Color.FromArgb(0x19, 0x00, 0x00, 0x00)
        };
    }
}