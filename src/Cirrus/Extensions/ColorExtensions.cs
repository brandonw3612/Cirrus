using System.Numerics;
using Wacton.Unicolour;
using WeightedColorPair = System.Collections.Generic.KeyValuePair<Wacton.Unicolour.Unicolour, int>;
using WeightedColor = (Wacton.Unicolour.Unicolour Color, double Weight);

namespace Cirrus.Extensions;

public static class ColorExtensions
{
    private const byte WhiteThreshold = 250;
    
    public static bool IsDark(this Unicolour color)
    {
        return color.Lab.L <= 55;
    }
    
    public static Vector3 ToSrgbVector3(this Unicolour color)
    {
        return new Vector3((float)color.Rgb.R, (float)color.Rgb.G, (float)color.Rgb.B);
    }
    
    public static Dictionary<Unicolour, int> ExcludeWhitesIf(this Dictionary<Unicolour, int> sourceColorMap, bool ignoreWhite)
    {
        var entries = ignoreWhite
            ? sourceColorMap.Where(t =>
                t.Value > 0 &&
                (t.Key.Rgb.Byte255.R < WhiteThreshold ||
                 t.Key.Rgb.Byte255.G < WhiteThreshold ||
                 t.Key.Rgb.Byte255.B < WhiteThreshold
                )).ToList()
            : sourceColorMap.Where(t => t.Value > 0).ToList();

        if (entries.Count == 0)
        {
            entries = sourceColorMap.Where(t => t.Value > 0).ToList();
        }

        var map = new Dictionary<Unicolour, int>();
        foreach (var (color, count) in entries)
        {
            if (count <= 0) continue;
            if (!map.TryAdd(color, count))
            {
                map[color] += count;
            }
        }

        return map;
    }
    
    public static Unicolour MixWeighted(this IEnumerable<WeightedColor> colors, ColourSpace colourSpace)
    {
        var valid = colors.Where(i => i.Weight > 0).ToList();
        
        if (valid.Count == 0) throw new ArgumentException("No colors with positive weight provided.", nameof(colors));
        
        double totalWeight = valid.Sum(i => i.Weight);
        
        double sum1 = 0, sum2 = 0, sum3 = 0, sumAlpha = 0;
        
        foreach (var (color, weight) in valid)
        {
            double w = weight / totalWeight;
            var (v1, v2, v3) = color.GetRepresentation(colourSpace);
            double vAlpha = color.Alpha.A;
            
            sum1 += v1 * w;
            sum2 += v2 * w;
            sum3 += v3 * w;
            sumAlpha += vAlpha * w;
        }

        return new(colourSpace, sum1, sum2, sum3, sumAlpha);
    }
}