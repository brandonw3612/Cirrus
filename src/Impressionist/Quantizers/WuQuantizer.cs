using Impressionist.Helpers;
using System.Numerics;

namespace Impressionist.Quantizers;

public class WuQuantizer : IQuantizer
{
    private int[] _weights = [];
    private int[] _momentsR = [];
    private int[] _momentsG = [];
    private int[] _momentsB = [];
    private double[] _moments = [];
    private Box[] _cubes = [];


    // A histogram of all the input colors is constructed. It has the shape of a
    // cube. The cube would be too large if it contained all 16 million colors:
    // historical best practice is to use 5 bits of the 8 in each channel,
    // reducing the histogram to a volume of ~32,000.
    private const int IndexBits = 5;
    private const int MaxIndex = 32;
    private const int SideLength = 33;
    private const int TotalSize = 35937;

    public QuantizerResult Quantize(List<ArgbColor> data, int colorCount, List<ArgbColor>? startingClusters = null)
    {
        var colors = new Dictionary<ArgbColor, int>();
        foreach (var color in data)
        {
            colors[color] = colors.TryGetValue(color, out var count) ? count + 1 : 1;
        }
        ConstructHistogram(colors);
        ComputeMoments();
        var createBoxesResult = CreateBoxes(colorCount);
        var results = CreateResult(createBoxesResult.ResultCount);

        var colorToCount = new Dictionary<ArgbColor, int>();
        foreach (var color in results)
            colorToCount[color] = 0;

        return new QuantizerResult(colorToCount);
    }
    private static int GetIndex(int r, int g, int b)
    {
        return (r << (IndexBits * 2)) + (r << (IndexBits + 1)) + (g << IndexBits) + r + g + b;
    }

    private void ConstructHistogram(IReadOnlyDictionary<ArgbColor, int> pixels)
    {
        _weights = new int[TotalSize];
        _momentsR = new int[TotalSize];
        _momentsG = new int[TotalSize];
        _momentsB = new int[TotalSize];
        _moments = new double[TotalSize];

        foreach (var entry in pixels)
        {
            var argb = entry.Key;
            var count = entry.Value;
            var red = argb.Red;
            var green = argb.Green;
            var blue = argb.Blue;
            var bitsToRemove = 8 - IndexBits;
            var iR = (red >> bitsToRemove) + 1;
            var iG = (green >> bitsToRemove) + 1;
            var iB = (blue >> bitsToRemove) + 1;
            var index = GetIndex(iR, iG, iB);
            _weights[index] += count;
            _momentsR[index] += red * count;
            _momentsG[index] += green * count;
            _momentsB[index] += blue * count;
            _moments[index] += count * (red * red + green * green + blue * blue);
        }
    }

    private void ComputeMoments()
    {
        for (var r = 1; r < SideLength; ++r)
        {
            var area = new int[SideLength];
            var areaR = new int[SideLength];
            var areaG = new int[SideLength];
            var areaB = new int[SideLength];
            var area2 = new double[SideLength];

            for (var g = 1; g < SideLength; g++)
            {
                var line = 0;
                var lineR = 0;
                var lineG = 0;
                var lineB = 0;
                var line2 = 0.0;

                for (var b = 1; b < SideLength; b++)
                {
                    var index = GetIndex(r, g, b);
                    line += _weights[index];
                    lineR += _momentsR[index];
                    lineG += _momentsG[index];
                    lineB += _momentsB[index];
                    line2 += _moments[index];

                    area[b] += line;
                    areaR[b] += lineR;
                    areaG[b] += lineG;
                    areaB[b] += lineB;
                    area2[b] += line2;

                    var previousIndex = GetIndex(r - 1, g, b);
                    _weights[index] = _weights[previousIndex] + area[b];
                    _momentsR[index] = _momentsR[previousIndex] + areaR[b];
                    _momentsG[index] = _momentsG[previousIndex] + areaG[b];
                    _momentsB[index] = _momentsB[previousIndex] + areaB[b];
                    _moments[index] = _moments[previousIndex] + area2[b];
                }
            }
        }
    }

    private CreateBoxesResult CreateBoxes(int maxColorCount)
    {
        _cubes = new Box[maxColorCount];
        for (var i = 0; i < maxColorCount; i++)
            _cubes[i] = new Box();

        _cubes[0] = new Box
        {
            R0 = 0,
            R1 = MaxIndex,
            G0 = 0,
            G1 = MaxIndex,
            B0 = 0,
            B1 = MaxIndex,
            Vol = 0
        };

        var volumeVariance = new double[maxColorCount];
        var next = 0;
        var generatedColorCount = maxColorCount;

        for (var i = 1; i < maxColorCount; i++)
        {
            if (Cut(_cubes[next], _cubes[i]))
            {
                volumeVariance[next] = _cubes[next].Vol > 1 ? Variance(_cubes[next]) : 0.0;
                volumeVariance[i] = _cubes[i].Vol > 1 ? Variance(_cubes[i]) : 0.0;
            }
            else
            {
                volumeVariance[next] = 0.0;
                i--;
            }

            next = 0;
            var temp = volumeVariance[0];
            for (var j = 1; j <= i; j++)
                if (volumeVariance[j] > temp)
                {
                    temp = volumeVariance[j];
                    next = j;
                }

            if (temp <= 0.0)
            {
                generatedColorCount = i + 1;
                break;
            }
        }

        return new CreateBoxesResult
        {
            RequestedCount = maxColorCount,
            ResultCount = generatedColorCount
        };
    }

    private List<ArgbColor> CreateResult(int colorCount)
    {
        var colors = new List<ArgbColor>();
        for (var i = 0; i < colorCount; ++i)
        {
            var cube = _cubes[i];
            var weight = Volume(cube, _weights);
            if (weight > 0)
            {
                var r = (int)
                    Math.Round(
                        (double)Volume(cube, _momentsR) / weight,
                        MidpointRounding.AwayFromZero
                    );
                var g = (int)
                    Math.Round(
                        (double)Volume(cube, _momentsG) / weight,
                        MidpointRounding.AwayFromZero
                    );
                var b = (int)
                    Math.Round(
                        (double)Volume(cube, _momentsB) / weight,
                        MidpointRounding.AwayFromZero
                    );
                var color = new ArgbColor(255, (byte)r, (byte)g, (byte)b);
                colors.Add(color);
            }
        }

        return colors;
    }

    private double Variance(Box cube)
    {
        var dr = Volume(cube, _momentsR);
        var dg = Volume(cube, _momentsG);
        var db = Volume(cube, _momentsB);
        var xx =
            _moments[GetIndex(cube.R1, cube.G1, cube.B1)]
            - _moments[GetIndex(cube.R1, cube.G1, cube.B0)]
            - _moments[GetIndex(cube.R1, cube.G0, cube.B1)]
            + _moments[GetIndex(cube.R1, cube.G0, cube.B0)]
            - _moments[GetIndex(cube.R0, cube.G1, cube.B1)]
            + _moments[GetIndex(cube.R0, cube.G1, cube.B0)]
            + _moments[GetIndex(cube.R0, cube.G0, cube.B1)]
            - _moments[GetIndex(cube.R0, cube.G0, cube.B0)];

        var hypotenuse = dr * dr + dg * dg + db * db;
        var volume = Volume(cube, _weights);
        return xx - hypotenuse / volume;
    }

    private bool Cut(Box one, Box two)
    {
        var wholeR = Volume(one, _momentsR);
        var wholeG = Volume(one, _momentsG);
        var wholeB = Volume(one, _momentsB);
        var wholeW = Volume(one, _weights);

        var maxRResult = Maximize(
            one,
            Direction.Red,
            one.R0 + 1,
            one.R1,
            wholeR,
            wholeG,
            wholeB,
            wholeW
        );
        var maxGResult = Maximize(
            one,
            Direction.Green,
            one.G0 + 1,
            one.G1,
            wholeR,
            wholeG,
            wholeB,
            wholeW
        );
        var maxBResult = Maximize(
            one,
            Direction.Blue,
            one.B0 + 1,
            one.B1,
            wholeR,
            wholeG,
            wholeB,
            wholeW
        );

        Direction cutDirection;
        var maxR = maxRResult.Maximum;
        var maxG = maxGResult.Maximum;
        var maxB = maxBResult.Maximum;

        if (maxR >= maxG && maxR >= maxB)
        {
            cutDirection = Direction.Red;
            if (maxRResult.CutLocation < 0)
                return false;
        }
        else if (maxG >= maxR && maxG >= maxB)
        {
            cutDirection = Direction.Green;
        }
        else
        {
            cutDirection = Direction.Blue;
        }

        two.R1 = one.R1;
        two.G1 = one.G1;
        two.B1 = one.B1;

        switch (cutDirection)
        {
            case Direction.Red:
                one.R1 = maxRResult.CutLocation;
                two.R0 = one.R1;
                two.G0 = one.G0;
                two.B0 = one.B0;
                break;
            case Direction.Green:
                one.G1 = maxGResult.CutLocation;
                two.R0 = one.R0;
                two.G0 = one.G1;
                two.B0 = one.B0;
                break;
            case Direction.Blue:
                one.B1 = maxBResult.CutLocation;
                two.R0 = one.R0;
                two.G0 = one.G0;
                two.B0 = one.B1;
                break;
        }

        one.Vol = (one.R1 - one.R0) * (one.G1 - one.G0) * (one.B1 - one.B0);
        two.Vol = (two.R1 - two.R0) * (two.G1 - two.G0) * (two.B1 - two.B0);
        return true;
    }

    private MaximizeResult Maximize(
        Box cube,
        Direction direction,
        int first,
        int last,
        int wholeR,
        int wholeG,
        int wholeB,
        int wholeW
    )
    {
        var bottomR = Bottom(cube, direction, _momentsR);
        var bottomG = Bottom(cube, direction, _momentsG);
        var bottomB = Bottom(cube, direction, _momentsB);
        var bottomW = Bottom(cube, direction, _weights);

        var max = 0.0;
        var cut = -1;

        for (var i = first; i < last; i++)
        {
            var halfR = bottomR + Top(cube, direction, i, _momentsR);
            var halfG = bottomG + Top(cube, direction, i, _momentsG);
            var halfB = bottomB + Top(cube, direction, i, _momentsB);
            var halfW = bottomW + Top(cube, direction, i, _weights);

            if (halfW == 0)
                continue;

            var tempNumerator = (double)(halfR * halfR + halfG * halfG + halfB * halfB);
            var tempDenominator = (double)halfW;
            var temp = tempNumerator / tempDenominator;

            halfR = wholeR - halfR;
            halfG = wholeG - halfG;
            halfB = wholeB - halfB;
            halfW = wholeW - halfW;

            if (halfW == 0)
                continue;

            tempNumerator = (double)(halfR * halfR + halfG * halfG + halfB * halfB);
            tempDenominator = (double)halfW;
            temp += tempNumerator / tempDenominator;

            if (temp > max)
            {
                max = temp;
                cut = i;
            }
        }

        return new MaximizeResult { CutLocation = cut, Maximum = max };
    }

    private static int Volume(Box cube, int[] moment)
    {
        return moment[GetIndex(cube.R1, cube.G1, cube.B1)]
               - moment[GetIndex(cube.R1, cube.G1, cube.B0)]
               - moment[GetIndex(cube.R1, cube.G0, cube.B1)]
               + moment[GetIndex(cube.R1, cube.G0, cube.B0)]
               - moment[GetIndex(cube.R0, cube.G1, cube.B1)]
               + moment[GetIndex(cube.R0, cube.G1, cube.B0)]
               + moment[GetIndex(cube.R0, cube.G0, cube.B1)]
               - moment[GetIndex(cube.R0, cube.G0, cube.B0)];
    }

    private static int Bottom(Box cube, Direction direction, int[] moment)
    {
        return direction switch
        {
            Direction.Red => -moment[GetIndex(cube.R0, cube.G1, cube.B1)]
                             + moment[GetIndex(cube.R0, cube.G1, cube.B0)]
                             + moment[GetIndex(cube.R0, cube.G0, cube.B1)]
                             - moment[GetIndex(cube.R0, cube.G0, cube.B0)],
            Direction.Green => -moment[GetIndex(cube.R1, cube.G0, cube.B1)]
                               + moment[GetIndex(cube.R1, cube.G0, cube.B0)]
                               + moment[GetIndex(cube.R0, cube.G0, cube.B1)]
                               - moment[GetIndex(cube.R0, cube.G0, cube.B0)],
            Direction.Blue => -moment[GetIndex(cube.R1, cube.G1, cube.B0)]
                              + moment[GetIndex(cube.R1, cube.G0, cube.B0)]
                              + moment[GetIndex(cube.R0, cube.G1, cube.B0)]
                              - moment[GetIndex(cube.R0, cube.G0, cube.B0)],
            _ => 0
        };
    }

    private static int Top(Box cube, Direction direction, int position, int[] moment)
    {
        return direction switch
        {
            Direction.Red => moment[GetIndex(position, cube.G1, cube.B1)]
                             - moment[GetIndex(position, cube.G1, cube.B0)]
                             - moment[GetIndex(position, cube.G0, cube.B1)]
                             + moment[GetIndex(position, cube.G0, cube.B0)],
            Direction.Green => moment[GetIndex(cube.R1, position, cube.B1)]
                               - moment[GetIndex(cube.R1, position, cube.B0)]
                               - moment[GetIndex(cube.R0, position, cube.B1)]
                               + moment[GetIndex(cube.R0, position, cube.B0)],
            Direction.Blue => moment[GetIndex(cube.R1, cube.G1, position)]
                              - moment[GetIndex(cube.R1, cube.G0, position)]
                              - moment[GetIndex(cube.R0, cube.G1, position)]
                              + moment[GetIndex(cube.R0, cube.G0, position)],
            _ => 0
        };
    }

}

internal enum Direction
{
    Red,
    Green,
    Blue
}

internal sealed class MaximizeResult
{
    public int CutLocation { get; set; }
    public double Maximum { get; set; }
}

internal sealed class CreateBoxesResult
{
    public int RequestedCount { get; set; }
    public int ResultCount { get; set; }
}

internal sealed class Box
{
    public int R0 { get; set; }
    public int R1 { get; set; }
    public int G0 { get; set; }
    public int G1 { get; set; }
    public int B0 { get; set; }
    public int B1 { get; set; }
    public int Vol { get; set; }

    override public string ToString()
    {
        return $"Box: R {R0} -> {R1} G {G0} -> {G1} B {B0} -> {B1} VOL = {Vol}";
    }
}