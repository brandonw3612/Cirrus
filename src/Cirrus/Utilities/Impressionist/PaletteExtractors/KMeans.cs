using Cirrus.Extensions;
using Cirrus.Utilities.Impressionist.Primitives;
using Wacton.Unicolour;

namespace Cirrus.Utilities.Impressionist.PaletteExtractors;

public class KMeansConfig: IPaletteExtractorConfig
{
    public bool IgnoreWhite { get; set; } = true;
    public bool UseKMeansPP { get; set; } = false;
}

// I'm really appreciate wieslawsoltes's PaletteGenerator. Which make this project possible.
public class KMeans : IPaletteExtractor<KMeansConfig>
{
    private const int MaxIterations = 100;
    private const double ConvergenceEpsilon = 1e-3;

    public static IPaletteExtractor<KMeansConfig> Default => field ??= new KMeans();

    private static readonly Unicolour Black = new(ColourSpace.Rgb255, 0, 0, 0);

    public Unicolour ExtractThemeColor(Dictionary<Unicolour, int> sourceColorMap, KMeansConfig? config)
    {
        config ??= new();

        if (sourceColorMap.Count == 0)
        {
            return Black;
        }

        var ignoreWhite = config.IgnoreWhite && sourceColorMap.Count > 1;
        var targetColor = sourceColorMap.ExcludeWhitesIf(ignoreWhite);
        if (targetColor.Count == 0)
        {
            return Black;
        }
        
        var clusters = KMeansCluster(targetColor, 1, config.UseKMeansPP);

        return clusters.Length == 0 ? Black : clusters[0];
    }

    public PaletteResult ExtractPalette(Dictionary<Unicolour, int> sourceColorMap, int colorCount, KMeansConfig? config)
    {
        config ??= new();

        if (colorCount <= 0 || sourceColorMap.Count == 0)
        {
            return new PaletteResult(Enumerable.Repeat(Black, Math.Max(colorCount, 0)).ToList(), Black);
        }

        var useKMeansPP = config.UseKMeansPP && sourceColorMap.Count > 1;
        var themeColor = ExtractThemeColor(sourceColorMap, config);

        List<KeyValuePair<Unicolour, int>> builder;
        if (themeColor.IsDark())
        {
            builder = sourceColorMap.Where(t => t.Value > 0 && t.Key.IsDark()).ToList();
            if (builder.Count == 0)
            {
                builder = sourceColorMap
                    .Where(t => t.Value > 0)
                    .Select(t =>
                    {
                        var (l, a, b) = t.Key.Lab;
                        return new KeyValuePair<Unicolour, int>(
                            new Unicolour(ColourSpace.Lab, Math.Min(l, 50), a, b),
                            t.Value
                        );
                    })
                    .ToList();
            }
        }
        else
        {
            builder = sourceColorMap.Where(t => t.Value > 0 && !t.Key.IsDark()).ToList();
            if (builder.Count == 0)
            {
                builder = sourceColorMap.Where(t => t.Value > 0).ToList();
            }
        }

        var targetColors = builder.ToDictionary(p => p.Key, p => p.Value);
        var clusters = KMeansCluster(targetColors, colorCount, useKMeansPP);
        if (clusters.Length == 0)
        {
            return new PaletteResult(Enumerable.Repeat(Black, colorCount).ToList(), themeColor);
        }

        var result = new List<Unicolour>(colorCount);
        var count = clusters.Length;
        for (int i = 0; i < colorCount; i++)
        {
            // You know, it is always hard to fulfill a palette when you have no enough colors. So please forgive me when placing the same color over and over again.
            result.Add(clusters[i % count]);
        }

        return new PaletteResult(result, themeColor);
    }

    private static Unicolour[] KMeansCluster(Dictionary<Unicolour, int> colors, int numClusters, bool useKMeansPP)
    {
        var clusterCount = Math.Min(numClusters, colors.Count);
        if (clusterCount <= 0)
        {
            return [];
        }

        var centers = !useKMeansPP
            ? colors.Keys.OrderByDescending(_ => Guid.NewGuid()).Take(clusterCount).ToArray()
            : KMeansPlusPlusCluster(colors, clusterCount);

        clusterCount = centers.Length;
        var sumL = new double[clusterCount];
        var sumA = new double[clusterCount];
        var sumB = new double[clusterCount];
        var weights = new long[clusterCount];

        var changed = true;
        int iterations = 0;
        while (changed && iterations < MaxIterations)
        {
            changed = false;
            iterations++;

            Array.Clear(sumL);
            Array.Clear(sumA);
            Array.Clear(sumB);
            Array.Clear(weights);

            foreach (var (color, weight) in colors)
            {
                int nearestIndex = 0;
                double minDiff = centers[0].Difference(color, DeltaE.Ok);
                for (int c = 1; c < clusterCount; c++)
                {
                    double diff = centers[c].Difference(color, DeltaE.Ok);
                    if (diff < minDiff)
                    {
                        minDiff = diff;
                        nearestIndex = c;
                    }
                }

                var (l, a, b) = color.Oklab;
                sumL[nearestIndex] += l * weight;
                sumA[nearestIndex] += a * weight;
                sumB[nearestIndex] += b * weight;
                weights[nearestIndex] += weight;
            }

            for (int i = 0; i < clusterCount; i++)
            {
                if (weights[i] <= 0) continue;

                var newCenter = new Unicolour(
                    ColourSpace.Oklab,
                    sumL[i] / weights[i],
                    sumA[i] / weights[i],
                    sumB[i] / weights[i]);

                if (newCenter.Difference(centers[i], DeltaE.Ok) > ConvergenceEpsilon)
                {
                    changed = true;
                }

                centers[i] = newCenter;
            }
        }

        return centers;
    }

    private static Unicolour[] KMeansPlusPlusCluster(Dictionary<Unicolour, int> colors, int numClusters)
    {
        var random = new Random();
        var clusterCount = Math.Min(numClusters, colors.Count);
        var entries = colors.ToArray();
        var clusters = new List<Unicolour>(clusterCount);

        long totalWeight = 0;
        foreach (var entry in entries)
        {
            totalWeight += entry.Value;
        }

        if (totalWeight <= 0)
        {
            return [];
        }

        var firstTarget = random.NextDouble() * totalWeight;
        long runningWeight = 0;
        int firstIndex = 0;
        for (int i = 0; i < entries.Length; i++)
        {
            runningWeight += entries[i].Value;
            if (runningWeight >= firstTarget)
            {
                firstIndex = i;
                break;
            }
        }

        clusters.Add(entries[firstIndex].Key);

        var accDistances = new double[entries.Length];
        for (int i = 1; i < clusterCount; i++)
        {
            double accumulatedDistances = 0.0;
            for (int vectorId = 0; vectorId < entries.Length; vectorId++)
            {
                var candidate = entries[vectorId].Key;
                var minDistance = clusters[0].Difference(candidate, DeltaE.Ok);
                for (int clusterIdx = 1; clusterIdx < i; clusterIdx++)
                {
                    var currentDistance = clusters[clusterIdx].Difference(candidate, DeltaE.Ok);
                    if (currentDistance < minDistance)
                    {
                        minDistance = currentDistance;
                    }
                }

                accumulatedDistances += entries[vectorId].Value * minDistance * minDistance;
                accDistances[vectorId] = accumulatedDistances;
            }

            if (accumulatedDistances <= 0.0)
            {
                break;
            }

            var targetPoint = random.NextDouble() * accumulatedDistances;
            for (int vectorId = 0; vectorId < entries.Length; vectorId++)
            {
                if (accDistances[vectorId] >= targetPoint)
                {
                    clusters.Add(entries[vectorId].Key);
                    break;
                }
            }
        }

        return clusters.ToArray();
    }
}