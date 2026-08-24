using System;
using System.Collections.Generic;
using System.Text;
using Impressionist.Helpers;
using Impressionist.Helpers.Hct;

namespace Impressionist.Selectors;

public class HctColorSelector : IColorSelector
{
    public double ChromaWeight { get; set; } = 1;
    public double ToneWeight { get; set; } = -0.75;
    public double PopulationWeight { get; set; } = 3;
    record HctColorItem(Hct Color, double Population)
    {
        public double Score { get; set; }
        public double ChomaScore { get; set; }
        public double ToneScore { get; set; }
        public double PopulationScore { get; set; }

        public override string ToString()
        {
            return $"{ChomaScore},{ToneScore},{PopulationScore} = {Score}";
        }
    }
    public List<ArgbColor> SelectColors(Dictionary<ArgbColor, int> colorsToPopulation, int desired = 4)
    {
        var hctColors = colorsToPopulation
            .Select(p => new HctColorItem(Hct.From(p.Key), p.Value))
            .ToList();
        var totalPopulation = hctColors.Sum(p => p.Population);
        var avgChroma = hctColors.Sum(p => p.Color.Chroma * p.Population / totalPopulation);
        var avgTone = hctColors.Sum(p => p.Color.Tone * p.Population / totalPopulation);
        foreach (var color in hctColors)
        {
            var chromaScore = -Math.Abs(color.Color.Chroma - avgChroma) + color.Color.Chroma * ChromaWeight;
            var toneScore = -Math.Abs(color.Color.Tone - avgTone) + color.Color.Tone * ToneWeight;
            var populationScore = color.Population / totalPopulation * 100 * PopulationWeight;
            color.ChomaScore = chromaScore;
            color.ToneScore = toneScore;
            color.PopulationScore = populationScore;
            color.Score = chromaScore + toneScore + populationScore;
        }
        var ordered = hctColors.OrderByDescending(p => p.Score);
        return ordered.Take(desired).Select(p => p.Color.Argb).ToList();
    }
}
