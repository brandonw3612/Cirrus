using Impressionist.Helpers;
using Impressionist.Helpers.Hct;
using System;
using System.Collections.Generic;
using System.Text;

namespace Impressionist;

public class Score
{
    private struct ScoredHct(Hct hct, double chromaScore, double weightScore) : IComparable<ScoredHct>
    {
        public Hct Hct = hct;
        public readonly double Score => ChromaScore + WeightScore;
        public readonly double ChromaScore = chromaScore;
        public readonly double WeightScore = weightScore;

        public int CompareTo(ScoredHct other)
        {
            return Score.CompareTo(other.Score);
        }
    }

    private const double WeightProportion = 0.7;

    private static double GetChromaScore(Hct color)
    {
        var chroma = color.Chroma;
        return chroma switch
        {
            < 16 => chroma * 0.05,
            < 48 => 16 * 0.05 + (chroma - 16) * 0.5,
            _ => 16 * 0.05 + (48 - 16) * 0.5 + (chroma - 48) * 0.4
        };
    }

    public static List<ArgbColor> CalculateScore(
        Dictionary<ArgbColor, int> colorsToPopulation,
        int desired = 4,
        ArgbColor? fallbackColorARGB = null,
        bool filter = true
    )
    {
        fallbackColorARGB ??= new ArgbColor(0xff4285F4);

        // Get the HCT color for each Argb value, while finding the per hue count and
        // total count.
        List<Hct> colorsHct = [];
        var huePopulation = Enumerable.Repeat(0, 360).ToList();
        var populationSum = 0;
        foreach (var entry in colorsToPopulation)
        {
            var argb = entry.Key;
            var population = entry.Value;
            var hct = Hct.From(argb);
            colorsHct.Add(hct);
            var hue = (int)Math.Floor(hct.Hue);
            huePopulation[hue] += population;
            populationSum += population;
        }

        // Hues with more usage in neighboring 30 degree slice get a larger number.
        var hueExcitedProportions = Enumerable.Repeat(0.0, 360).ToList();
        for (var hue = 0; hue < 360; hue++)
        {
            var proportion = huePopulation[hue] / (double)populationSum;
            for (var i = hue - 14; i < hue + 16; i++)
            {
                var neighborHue = (int)MathUtils.SanitizeDegrees(i);
                hueExcitedProportions[neighborHue] += proportion;
            }
        }

        // Scores each HCT color based on usage and chroma, while optionally
        // filtering out values that do not have enough chroma or usage.
        List<ScoredHct> scoredHcts = [];
        foreach (var hct in colorsHct)
        {
            var hue = (int)
                MathUtils.SanitizeDegrees(Math.Round(hct.Hue, MidpointRounding.AwayFromZero));
            var proportion = hueExcitedProportions[hue];

            var proportionScore = proportion * 100.0 * WeightProportion;
            var chromaScore = GetChromaScore(hct);
            scoredHcts.Add(new ScoredHct(hct, chromaScore, proportionScore));
        }

        // Sorted so that colors with higher scores come first.
        scoredHcts.Sort();
        scoredHcts.Reverse();

        // Iterates through potential hue differences in degrees in order to select
        // the colors with the largest distribution of hues possible. Starting at
        // 90 degrees(maximum difference for 4 colors) then decreasing down to a
        // 15 degree minimum.
        List<ScoredHct> chosenColors = [];
        foreach(var scoredHct in scoredHcts)
        {
            if(chosenColors.Count >= desired)
            {
                break;
            }
            if (chosenColors.Count >= 2 && chosenColors.Last().ChromaScore - scoredHct.ChromaScore >= chosenColors.Last().ChromaScore *0.6)
            {
                break;
            }
            if (chosenColors.Count == 1 && chosenColors.Last().ChromaScore - scoredHct.ChromaScore >= chosenColors.Last().ChromaScore * 0.6)
                continue;
            chosenColors.Add(scoredHct);
        }

        //chosenColors = SpreadIfColorsTooSimilar(chosenColors);

        List<ArgbColor> colors = [];
        if (!chosenColors.Any())
            colors.Add(fallbackColorARGB.Value);

        colors.AddRange(chosenColors.Select(chosenHct => chosenHct.Hct.Argb));
        return colors;
    }

}