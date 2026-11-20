using Cirrus.Extensions;
using Cirrus.Utilities.Impressionist.Primitives;
using Wacton.Unicolour;

namespace Cirrus.Utilities.Impressionist.PaletteExtractors;

public class MondrianConfig : IPaletteExtractorConfig
{
    public const double DefaultMinContrastWithWhite = 4.5;

    public bool IgnoreWhite { get; set; } = true;
    public double MinContrastWithWhite { get; set; } = DefaultMinContrastWithWhite;
}

public class Mondrian : IPaletteExtractor<MondrianConfig>
{
    private const int MaxKMeansIterations = 30;
    private const double ConvergenceEpsilon = 1e-3;
    private const double MinAccentWeightRatio = 0.015;
    private const double StrictMinOkLabDistance = 0.06;
    private const double RelaxedMinOkLabDistance = 0.025;

    private readonly record struct WeightedColorSample(Unicolour Color, int Weight);

    private readonly record struct ColorCluster(Unicolour Center, long TotalWeight);

    private readonly record struct AdjustedCandidate(Unicolour Color, long TotalWeight, double WeightRatio, double Chroma);

    public static IPaletteExtractor<MondrianConfig> Default => field ??= new Mondrian();

    private static Unicolour Black => field ??= new Unicolour(ColourSpace.Rgb255, 0, 0, 0);
    private static Unicolour White => field ??= new Unicolour(ColourSpace.Rgb255, 255, 255, 255);

    public Unicolour ExtractThemeColor(Dictionary<Unicolour, int> sourceColorMap, MondrianConfig? config)
    {
        config ??= new();

        if (sourceColorMap.Count == 0)
        {
            return Black;
        }

        var samples = BuildWeightedSamples(sourceColorMap, config.IgnoreWhite && sourceColorMap.Count > 1);

        if (samples.Length == 0)
        {
            return Black;
        }

        var overClusterCount = Math.Min(samples.Length, 12);
        var clusters = RunWeightedKMeansOkLab(samples, overClusterCount);
        return EnsureContrastWithWhite(clusters[0].Center, config.MinContrastWithWhite);
    }

    public PaletteResult ExtractPalette(Dictionary<Unicolour, int> sourceColorMap, int colorCount, MondrianConfig? config)
    {
        config ??= new();

        if (colorCount <= 0 || sourceColorMap is null || sourceColorMap.Count == 0)
        {
            return new PaletteResult(Enumerable.Repeat(Black, Math.Max(colorCount, 0)).ToList(), Black);
        }

        var samples = BuildWeightedSamples(sourceColorMap, config.IgnoreWhite && sourceColorMap.Count > 1);
        if (samples.Length == 0)
        {
            samples = BuildWeightedSamples(sourceColorMap, ignoreWhite: false);
        }

        if (samples.Length == 0)
        {
            return new PaletteResult(Enumerable.Repeat(Black, colorCount).ToList(), Black);
        }

        var overClusterCount = Math.Min(samples.Length, Math.Max(colorCount * 3, 12));
        var clusters = RunWeightedKMeansOkLab(samples, overClusterCount);

        long totalWeight = 0;
        foreach (var cluster in clusters)
        {
            totalWeight += cluster.TotalWeight;
        }

        var minContrast = config.MinContrastWithWhite;
        var candidates = new List<AdjustedCandidate>(clusters.Count);
        foreach (var cluster in clusters)
        {
            var adjusted = EnsureContrastWithWhite(cluster.Center, minContrast);
            var chroma = adjusted.Oklch.C;
            var ratio = totalWeight > 0 ? (double)cluster.TotalWeight / totalWeight : 0.0;
            candidates.Add(new AdjustedCandidate(adjusted, cluster.TotalWeight, ratio, double.IsFinite(chroma) ? chroma : 0.0));
        }

        var palette = SelectDiversePalette(candidates, colorCount, minContrast);
        return new PaletteResult(palette, candidates[0].Color);
    }

    private static WeightedColorSample[] BuildWeightedSamples(Dictionary<Unicolour, int> sourceColorMap, bool ignoreWhite)
    {
        var entries = sourceColorMap.ExcludeWhitesIf(ignoreWhite);
        var list = new List<WeightedColorSample>(sourceColorMap.Count);

        foreach (var (color, count) in entries)
        {
            if (count <= 0) continue;

            var (l, a, b) = color.Oklab;
            if (!double.IsFinite(l) || !double.IsFinite(a) || !double.IsFinite(b)) continue;

            list.Add(new WeightedColorSample(color, count));
        }

        return [.. list];
    }

    private static List<ColorCluster> RunWeightedKMeansOkLab(WeightedColorSample[] samples, int k)
    {
        if (samples.Length == 1 || k <= 1)
        {
            long totalWeight = 0;
            foreach (var sample in samples)
            {
                totalWeight += sample.Weight;
            }

            var mean = samples
                .Select(s => (s.Color, (double)s.Weight))
                .MixWeighted(ColourSpace.Oklab);
            return [new ColorCluster(mean, totalWeight)];
        }

        var centers = InitializeWeightedKMeansPlusPlus(samples, k);
        int actualK = centers.Count;
        var centerArray = centers.ToArray();
        var sumL = new double[actualK];
        var sumA = new double[actualK];
        var sumB = new double[actualK];
        var weights = new long[actualK];

        for (int iter = 0; iter < MaxKMeansIterations; iter++)
        {
            Array.Clear(sumL);
            Array.Clear(sumA);
            Array.Clear(sumB);
            Array.Clear(weights);

            foreach (var sample in samples)
            {
                var (sl, sa, sb) = sample.Color.Oklab;
                int nearestIndex = 0;
                double minDistSq = OkLabDistanceSquared(sl, sa, sb, centerArray[0].Oklab);

                for (int c = 1; c < actualK; c++)
                {
                    double distSq = OkLabDistanceSquared(sl, sa, sb, centerArray[c].Oklab);
                    if (distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        nearestIndex = c;
                    }
                }

                sumL[nearestIndex] += sl * sample.Weight;
                sumA[nearestIndex] += sa * sample.Weight;
                sumB[nearestIndex] += sb * sample.Weight;
                weights[nearestIndex] += sample.Weight;
            }

            bool converged = true;
            for (int c = 0; c < actualK; c++)
            {
                if (weights[c] <= 0) continue;

                var nextCenter = new Unicolour(
                    ColourSpace.Oklab,
                    sumL[c] / weights[c],
                    sumA[c] / weights[c],
                    sumB[c] / weights[c]);

                if (nextCenter.Difference(centerArray[c], DeltaE.Ok) > ConvergenceEpsilon)
                {
                    converged = false;
                }

                centerArray[c] = nextCenter;
            }

            if (converged) break;
        }

        var result = new List<ColorCluster>(actualK);
        for (int c = 0; c < actualK; c++)
        {
            if (weights[c] > 0)
            {
                result.Add(new ColorCluster(centerArray[c], weights[c]));
            }
        }

        result.Sort((a, b) => b.TotalWeight.CompareTo(a.TotalWeight));
        return result;
    }

    private static List<Unicolour> InitializeWeightedKMeansPlusPlus(WeightedColorSample[] samples, int k)
    {
        var random = new Random(42);
        var centers = new List<Unicolour>(k);

        int maxWeightIndex = 0;
        for (int i = 1; i < samples.Length; i++)
        {
            if (samples[i].Weight > samples[maxWeightIndex].Weight)
            {
                maxWeightIndex = i;
            }
        }

        centers.Add(samples[maxWeightIndex].Color);

        var minDistSq = new double[samples.Length];
        Array.Fill(minDistSq, double.PositiveInfinity);

        var cumulativeWeights = new double[samples.Length];

        for (int c = 1; c < k; c++)
        {
            var lastLab = centers[^1].Oklab;
            double totalScore = 0.0;

            for (int i = 0; i < samples.Length; i++)
            {
                var (sl, sa, sb) = samples[i].Color.Oklab;
                double distSq = OkLabDistanceSquared(sl, sa, sb, lastLab);
                if (distSq < minDistSq[i])
                {
                    minDistSq[i] = distSq;
                }

                totalScore += samples[i].Weight * minDistSq[i];
                cumulativeWeights[i] = totalScore;
            }

            if (totalScore <= 0.0) break;

            double target = random.NextDouble() * totalScore;
            int chosenIndex = Array.BinarySearch(cumulativeWeights, target);
            if (chosenIndex < 0)
            {
                chosenIndex = ~chosenIndex;
            }

            chosenIndex = Math.Clamp(chosenIndex, 0, samples.Length - 1);
            centers.Add(samples[chosenIndex].Color);
        }

        return centers;
    }

    private static List<Unicolour> SelectDiversePalette(
        List<AdjustedCandidate> candidates,
        int slotCount,
        double minContrastWithWhite)
    {
        var selectedIndices = new List<int>(slotCount) { 0 };
        var selectedColors = new List<Unicolour>(slotCount) { candidates[0].Color };

        var dominantL = candidates[0].Color.Oklab.L;
        bool isDarkCover = dominantL < 0.35;

        FillDiverseSlots(
            candidates,
            selectedIndices,
            selectedColors,
            slotCount,
            dominantL,
            isDarkCover,
            minWeightRatio: MinAccentWeightRatio,
            minRawDistance: StrictMinOkLabDistance,
            skipTinyAchromaticOutliers: true);

        if (selectedColors.Count < slotCount)
        {
            FillDiverseSlots(
                candidates,
                selectedIndices,
                selectedColors,
                slotCount,
                dominantL,
                isDarkCover,
                minWeightRatio: 0.008,
                minRawDistance: RelaxedMinOkLabDistance,
                skipTinyAchromaticOutliers: true);
        }

        int variantStep = 0;
        while (selectedColors.Count < slotCount)
        {
            var baseColor = selectedColors[variantStep % selectedColors.Count];
            selectedColors.Add(CreateHarmoniousVariation(baseColor, variantStep, minContrastWithWhite));
            variantStep++;
        }

        return selectedColors;
    }

    private static void FillDiverseSlots(
        List<AdjustedCandidate> candidates,
        List<int> selectedIndices,
        List<Unicolour> selectedColors,
        int slotCount,
        double dominantL,
        bool isDarkCover,
        double minWeightRatio,
        double minRawDistance,
        bool skipTinyAchromaticOutliers)
    {
        while (selectedColors.Count < slotCount)
        {
            int bestIndex = -1;
            double bestScore = -1.0;

            for (int i = 0; i < candidates.Count; i++)
            {
                if (selectedIndices.Contains(i)) continue;

                var candidate = candidates[i];
                if (candidate.WeightRatio < minWeightRatio) continue;

                if (skipTinyAchromaticOutliers
                    && isDarkCover
                    && candidate.WeightRatio < 0.08
                    && candidate.Color.Oklab.L > dominantL + 0.22
                    && candidate.Chroma < 0.04)
                {
                    continue;
                }

                double minRawDist = double.PositiveInfinity;
                double minPerceptualDist = double.PositiveInfinity;

                foreach (var chosen in selectedColors)
                {
                    double rawDist = candidate.Color.Difference(chosen, DeltaE.Ok);
                    double perceptualDist = ComputeChromaticWeightedDistance(candidate.Color, chosen);
                    if (rawDist < minRawDist) minRawDist = rawDist;
                    if (perceptualDist < minPerceptualDist) minPerceptualDist = perceptualDist;
                }

                if (minRawDist < minRawDistance) continue;

                double chromaBonus = 1.0 + 3.5 * candidate.Chroma;
                double weightFactor = Math.Pow(candidate.WeightRatio, 0.35);
                double score = minPerceptualDist * chromaBonus * weightFactor;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }

            if (bestIndex < 0) break;

            selectedIndices.Add(bestIndex);
            selectedColors.Add(candidates[bestIndex].Color);
        }
    }

    private static double ComputeChromaticWeightedDistance(Unicolour first, Unicolour second)
    {
        var (l1, a1, b1) = first.Oklab;
        var (l2, a2, b2) = second.Oklab;
        double dL = l1 - l2;
        double da = a1 - a2;
        double db = b1 - b2;
        return Math.Sqrt(0.55 * dL * dL + 2.2 * (da * da + db * db));
    }

    private static double OkLabDistanceSquared(double l, double a, double b, Oklab other)
    {
        double dL = l - other.L;
        double da = a - other.A;
        double db = b - other.B;
        return dL * dL + da * da + db * db;
    }

    private static Unicolour CreateHarmoniousVariation(Unicolour baseColor, int step, double minContrastWithWhite)
    {
        var (l, c, h) = baseColor.Oklch;
        double lightnessDelta = (step % 2 == 0 ? 0.06 : -0.05) * (1.0 + 0.25 * step / 2);
        double hueDelta = step % 2 == 0 ? 14.0 : -14.0;

        double nextL = Math.Clamp(l + lightnessDelta, 0.04, 0.95);
        double safeC = double.IsFinite(c) ? c : 0.0;
        double nextH = double.IsFinite(h) ? (h + hueDelta + 360.0) % 360.0 : 0.0;

        var varied = new Unicolour(ColourSpace.Oklch, nextL, safeC, nextH)
            .MapToRgbGamut(GamutMap.OklchChromaReduction);

        return EnsureContrastWithWhite(varied, minContrastWithWhite);
    }

    private static Unicolour EnsureContrastWithWhite(Unicolour color, double minContrast)
    {
        var inGamut = color.IsInRgbGamut
            ? color
            : color.MapToRgbGamut(GamutMap.OklchChromaReduction);

        double clampedContrast = Math.Clamp(minContrast, 1.0, 21.0);
        if (clampedContrast <= 1.0 || inGamut.Contrast(White) >= clampedContrast)
        {
            return inGamut;
        }

        if (clampedContrast >= 21.0)
        {
            return Black;
        }

        var (l, a, b) = inGamut.Oklab;
        double lowL = 0.0;
        double highL = Math.Clamp(l, 0.0, 1.0);
        var best = Black;

        for (int i = 0; i < 18; i++)
        {
            double midL = 0.5 * (lowL + highL);
            var candidate = new Unicolour(ColourSpace.Oklab, midL, a, b)
                .MapToRgbGamut(GamutMap.OklchChromaReduction);

            if (candidate.Contrast(White) >= clampedContrast)
            {
                best = candidate;
                lowL = midL;
            }
            else
            {
                highL = midL;
            }
        }

        return best;
    }
}
