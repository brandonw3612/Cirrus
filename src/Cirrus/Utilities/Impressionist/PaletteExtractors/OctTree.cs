using Cirrus.Extensions;
using Cirrus.Utilities.Impressionist.Primitives;
using Wacton.Unicolour;

namespace Cirrus.Utilities.Impressionist.PaletteExtractors;

public class OctTreeConfig : IPaletteExtractorConfig
{
    public bool IgnoreWhite { get; set; } = true;
}

public class OctTree : IPaletteExtractor<OctTreeConfig>
{
    public static IPaletteExtractor<OctTreeConfig> Default => field ??= new OctTree();

    private static readonly Unicolour Black = new(ColourSpace.Rgb255, 0, 0, 0);

    public Unicolour ExtractThemeColor(Dictionary<Unicolour, int> sourceColorMap, OctTreeConfig? config)
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

        var quantizer = new PaletteQuantizer();
        foreach (var (color, count) in targetColor)
        {
            quantizer.AddColorRange(color, count);
        }

        quantizer.Quantize(1);
        return quantizer.GetThemeResult();
    }

    public PaletteResult ExtractPalette(Dictionary<Unicolour, int> sourceColorMap, int colorCount, OctTreeConfig? config)
    {
        config ??= new();

        if (colorCount <= 0 || sourceColorMap.Count == 0)
        {
            return new PaletteResult(Enumerable.Repeat(Black, Math.Max(colorCount, 0)).ToList(), Black);
        }

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

        var targetColor = builder.ToDictionary(t => t.Key, t => t.Value);
        if (targetColor.Count == 0)
        {
            return new PaletteResult(Enumerable.Repeat(Black, colorCount).ToList(), themeColor);
        }

        var quantizer = new PaletteQuantizer();
        foreach (var (color, count) in targetColor)
        {
            quantizer.AddColorRange(color, count);
        }

        quantizer.Quantize(colorCount);
        var quantizeResult = quantizer.GetPaletteResult(colorCount);
        if (quantizeResult.Count == 0)
        {
            return new PaletteResult(Enumerable.Repeat(Black, colorCount).ToList(), themeColor);
        }

        List<Unicolour> result;
        if (quantizeResult.Count < colorCount)
        {
            result = new List<Unicolour>(colorCount);
            for (int i = 0; i < colorCount; i++)
            {
                result.Add(quantizeResult[i % quantizeResult.Count]);
            }
        }
        else
        {
            result = quantizeResult;
        }

        return new PaletteResult(result, themeColor);
    }

    private class PaletteQuantizer
    {
        private readonly Node _root;
        private readonly Dictionary<int, List<Node>> _levelNodes;

        public PaletteQuantizer()
        {
            _root = new Node(this);
            _levelNodes = new Dictionary<int, List<Node>>();
            for (int i = 0; i < 8; i++)
            {
                _levelNodes[i] = new List<Node>();
            }
        }

        public void AddColor(Unicolour color)
        {
            _root.AddColor(color, 0);
        }

        public void AddColorRange(Unicolour color, int count)
        {
            if (count <= 0) return;
            _root.AddColorRange(color, 0, count);
        }

        public void AddLevelNode(Node node, int level)
        {
            _levelNodes[level].Add(node);
        }

        public List<Unicolour> GetPaletteResult()
        {
            return _root.GetPaletteResult().Keys.ToList();
        }

        public List<Unicolour> GetPaletteResult(int count)
        {
            return _root.GetPaletteResult()
                .OrderByDescending(t => t.Value)
                .Take(count)
                .Select(t => t.Key)
                .ToList();
        }

        public Unicolour GetThemeResult()
        {
            var palette = _root.GetPaletteResult();
            if (palette.Count == 0)
            {
                return Black;
            }

            return palette
                .Select(t => (t.Key, (double)t.Value))
                .MixWeighted(ColourSpace.Oklab);
        }

        public void Quantize(int colorCount)
        {
            if (colorCount <= 0) return;

            var nodesToRemove = _levelNodes[7].Count - colorCount;
            int level = 6;
            var toBreak = false;
            while (level >= 0 && nodesToRemove > 0)
            {
                var leaves = _levelNodes[level]
                    .Where(n => n.ChildrenCount > 0 && n.ChildrenCount - 1 <= nodesToRemove)
                    .OrderBy(n => n.ChildrenCount);

                foreach (var leaf in leaves)
                {
                    int reduction = leaf.ChildrenCount - 1;
                    if (reduction > nodesToRemove)
                    {
                        toBreak = true;
                        continue;
                    }

                    nodesToRemove -= reduction;
                    leaf.Merge();
                    if (nodesToRemove <= 0)
                    {
                        break;
                    }
                }

                _levelNodes.Remove(level + 1);
                level--;
                if (toBreak)
                {
                    break;
                }
            }

            if (nodesToRemove > 0 && _root.ChildrenCount > 1 && _root.ChildrenCount - 1 <= nodesToRemove)
            {
                _root.Merge();
                _levelNodes.Remove(0);
            }
        }
    }

    private class Node
    {
        private readonly PaletteQuantizer _parent;
        private Node?[] _children = new Node[8];
        private Unicolour _color = Black;
        private int _count;

        public int ChildrenCount => _children.OfType<Node>().Count();

        public Node(PaletteQuantizer parent)
        {
            _parent = parent;
        }

        public void AddColor(Unicolour color, int level)
        {
            if (level < 8)
            {
                var index = GetIndex(color, level);
                if (_children[index] is null)
                {
                    var newNode = new Node(_parent);
                    _children[index] = newNode;
                    _parent.AddLevelNode(newNode, level);
                }
                _children[index]!.AddColor(color, level + 1);
            }
            else
            {
                _color = color;
                _count++;
            }
        }

        public void AddColorRange(Unicolour color, int level, int count)
        {
            if (level < 8)
            {
                var index = GetIndex(color, level);
                if (_children[index] is null)
                {
                    var newNode = new Node(_parent);
                    _children[index] = newNode;
                    _parent.AddLevelNode(newNode, level);
                }
                _children[index]!.AddColorRange(color, level + 1, count);
            }
            else
            {
                _color = color;
                _count += count;
            }
        }

        public Unicolour? GetColor(Unicolour color, int level)
        {
            if (ChildrenCount == 0)
            {
                return _count > 0 ? _color : null;
            }

            var index = GetIndex(color, level);
            return _children[index]?.GetColor(color, level + 1);
        }

        public Dictionary<Unicolour, int> GetPaletteResult()
        {
            var children = _children.OfType<Node>().ToArray();
            var result = new Dictionary<Unicolour, int>();
            if (children.Length == 0)
            {
                if (_count > 0 && !result.TryAdd(_color, _count))
                {
                    result[_color] += _count;
                }
            }
            else
            {
                foreach (var child in children)
                {
                    child.NodeGetResult(result);
                }
            }

            return result;
        }

        private void NodeGetResult(Dictionary<Unicolour, int> result)
        {
            var children = _children.OfType<Node>().ToArray();
            if (children.Length == 0)
            {
                if (_count > 0 && !result.TryAdd(_color, _count))
                {
                    result[_color] += _count;
                }
            }
            else
            {
                foreach (var child in children)
                {
                    child.NodeGetResult(result);
                }
            }
        }

        private static byte GetIndex(Unicolour color, int level)
        {
            byte ret = 0;
            var mask = Convert.ToByte(0b10000000 >> level);
            if ((color.Rgb.Byte255.R & mask) != 0)
            {
                ret |= 0b100;
            }
            if ((color.Rgb.Byte255.G & mask) != 0)
            {
                ret |= 0b010;
            }
            if ((color.Rgb.Byte255.B & mask) != 0)
            {
                ret |= 0b001;
            }
            return ret;
        }

        public void Merge()
        {
            var validChildren = _children
                .OfType<Node>()
                .Where(n => n._count > 0)
                .ToArray();

            if (validChildren.Length == 0)
            {
                _color = Black;
                _count = 0;
            }
            else
            {
                _color = validChildren
                    .Select(n => (Color: n._color, (double)n._count))
                    .MixWeighted(ColourSpace.Oklab);
                _count = validChildren.Sum(n => n._count);
            }

            _children = new Node[8];
        }
    }
}