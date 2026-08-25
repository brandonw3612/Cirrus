using Impressionist.Helpers;
using System.Numerics;

namespace Impressionist;

public interface IQuantizer
{
    QuantizerResult Quantize(
        List<ArgbColor> data,
        int colorCount,
        List<ArgbColor>? startingClusters = null);
}