using Impressionist.Helpers;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Impressionist.Quantizers;

/// <summary>
/// Implementation of the Weighted k-means (ws-means) algorithm for color quantization.
/// https://arxiv.org/abs/1101.0395
/// </summary>
public class WsMeansQuantizer : IQuantizer
{
    private const int MaxIterations = 5;

    public bool UseLab { get; set; } = true;

    public QuantizerResult Quantize(
        List<ArgbColor> dataColor,
        int colorCount,
        List<ArgbColor>? startingClusters = null)
    {
        var pointCount = dataColor.Count;
        if (startingClusters is null || startingClusters.Count == 0)
        {
            startingClusters = new List<ArgbColor>(colorCount);
            var firstIndex = Random.Shared.Next(pointCount);
            startingClusters.Add(dataColor[firstIndex]);
        }
        
        var clusters = new List<Vector3>(startingClusters.Count);
        foreach(var cluster in startingClusters)
        {
            clusters.Add(ArgbToVector(cluster));
        }

        var data = new List<Vector3>(dataColor.Count);
        foreach(var color in dataColor)
        {
            data.Add(ArgbToVector(color));
        }

        // K-Means++ initialization
        var distances = new float[pointCount];
        while (clusters.Count < colorCount)
        {
            var sum = 0f;
            for (var i = 0; i < pointCount; i++)
            {
                var minDist = float.MaxValue;
                foreach (var c in clusters)
                {
                    var dist = Vector3.DistanceSquared(data[i], c);
                    minDist = Math.Min(minDist, dist);
                }

                distances[i] = minDist;
                sum += minDist;
            }

            var target = Random.Shared.NextDouble() * sum;
            var currentSum = 0f;
            for (var i = 0; i < pointCount; i++)
            {
                currentSum += distances[i];
                if (currentSum >= target)
                {
                    clusters.Add(data[i]);
                    break;
                }
            }
        }


        var labels = new int[pointCount];
        for (var i = 0; i < pointCount; i++)
        {
            labels[i] = i % colorCount;
        }

        for (var t = 0; t < MaxIterations; t++)
        {
            var d = new double[colorCount, colorCount];
            for (var i = 0; i < colorCount; i++)
            {
                for (var j = i + 1; j < colorCount; j++)
                {
                    d[j, i] = d[i, j] = Vector3.DistanceSquared(clusters[i], clusters[j]);
                }
            }
            var m = new int[colorCount][];

            for (var i = 0; i < colorCount; i++)
            {
                m[i] = new int[colorCount];
                for (var j = 0; j < colorCount; j++)
                {
                    m[i][j] = j;
                }
            }

            for (var i = 0; i < colorCount; i++)
            {
                Array.Sort(m[i], (a, b) => d[i, a].CompareTo(d[i, b]));
            }

            for (var i = 0; i < pointCount; i++)
            {
                var prevCluster = labels[i];
                var prevDist = Vector3.DistanceSquared(data[i], clusters[prevCluster]);
                var minDist = prevDist;
                var minIndex = m[prevCluster][0];
                for (var j = 0; j < colorCount; j++)
                {

                    var distBetweenCenters = d[prevCluster, m[prevCluster][j]];
                    if (distBetweenCenters >= 4 * prevDist) break;
                    var dist = Vector3.DistanceSquared(data[i], clusters[m[prevCluster][j]]);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        minIndex = m[prevCluster][j];
                    }
                }
                labels[i] = minIndex;
            }

            var hasConverged = true;

            for (var j = 0; j < colorCount; j++)
            {
                var sum = Vector3.Zero;
                var pointsCount = 0;
                for (var i = 0; i < pointCount; i++)
                {
                    if (labels[i] == j)
                    {
                        sum += data[i];
                        pointsCount++;
                    }
                }
                if (pointsCount > 0)
                {
                    var newCluster = sum / pointsCount;
                    if (Vector3.DistanceSquared(clusters[j], newCluster) > 0.01f)
                    {
                        hasConverged = false;
                    }
                    clusters[j] = newCluster;
                }
            }

            if (hasConverged)
            {
                break;
            }
        }
        var dic = new Dictionary<ArgbColor, int>(colorCount);
        for (var i = 0; i < colorCount; i++)
        {
            dic[VectorToArgb(clusters[i])] = 0;
        }
        for (var i = 0; i < pointCount; i++)
        {
            dic[VectorToArgb(clusters[labels[i]])]++;
        }

        return new QuantizerResult(dic);
    }
    private Vector3 ArgbToVector(ArgbColor color)
    {
        if (UseLab)
            return ColorUtils.LabFromArgb(color);
        else
            return new Vector3(color.Red, color.Green, color.Blue);
    }
    private ArgbColor VectorToArgb(Vector3 v)
    {
        if (UseLab)
            return ColorUtils.ArgbFromLab(v);
        else
            return new ArgbColor(255, (byte)v.X, (byte)v.Y, (byte)v.Z);
    }
}