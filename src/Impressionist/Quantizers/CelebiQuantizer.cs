using Impressionist.Helpers;

namespace Impressionist.Quantizers;

public class CelebiQuantizer : IQuantizer
{
    public QuantizerResult Quantize(List<ArgbColor> data, int colorCount, List<ArgbColor>? clusters = null)
    {
        var startingClusters = new WuQuantizer().Quantize(data, colorCount, clusters).Colors.Keys.ToList();
        return new WsMeansQuantizer().Quantize(data, colorCount, startingClusters);
    }
}