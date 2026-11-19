using Cirrus.Helpers.Impressionist.Impressionist.Abstractions;
using System.Numerics;

namespace Cirrus.Helpers.Impressionist.Impressionist.Implementations
{
    public static class AutoPaletteGenerator
    {
        public static async Task<PaletteResult> CreatePalette(Dictionary<Vector3, int> sourceColor, int clusterCount, bool ignoreWhite = false, bool toLab = false, bool useKMeansPP = false)
        {
            var kmeansTask = PaletteGenerators.KMeansPaletteGenerator.CreatePalette(sourceColor, clusterCount, ignoreWhite, toLab, useKMeansPP);
            var octTreeTask = PaletteGenerators.OctTreePaletteGenerator.CreatePalette(sourceColor, clusterCount, ignoreWhite, true);

            await Task.WhenAll(kmeansTask, octTreeTask);

            var kmeansResult = kmeansTask.Result;
            var octTreeResult = octTreeTask.Result;

            var kMeansDiversity = CalculateSpatialDiversity(kmeansResult.Palette);
            var octTreeDiversity = CalculateSpatialDiversity(octTreeResult.Palette);

            return kMeansDiversity > octTreeDiversity ? kmeansResult : octTreeResult;
        }

        private static double CalculateSpatialDiversity(List<Vector3> palette)
        {
            if (palette == null || palette.Count == 0) return 0;

            var labVectors = palette.Select(t => t.RGBVectorToLABVector()).ToList();
            var centroid = Vector3.Zero;
            foreach (var vector in labVectors)
            {
                centroid += vector;
            }
            centroid /= labVectors.Count;

            var sumSquaredDistances = labVectors.Sum(v => Vector3.DistanceSquared(v, centroid));

            return sumSquaredDistances / labVectors.Count;
        }
    }
}
