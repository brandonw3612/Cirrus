using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Impressionist.Helpers.Hct;
/// <summary>
/// CAM16, a color appearance model. Colors are not just defined by their hex
/// code, but rather, a hex code and viewing conditions.
///
/// CAM16 instances also have coordinates in the CAM16-UCS space, called J*, a*,
/// b*, or jstar, astar, bstar in code. CAM16-UCS is included in the CAM16
/// specification, and should be used when measuring distances between colors.
///
/// In traditional color spaces, a color can be identified solely by the
/// observer's measurement of the color. Color appearance models such as CAM16
/// also use information about the environment where the color was
/// observed, known as the viewing conditions.
///
/// For example, white under the traditional assumption of a midday sun white
/// point is accurately measured as a slightly chromatic blue by CAM16.
/// (roughly, hue 203, chroma 3, lightness 100)
/// </summary>
public readonly record struct Cam16(
    // Like red, orange, yellow, green, etc.
    float Hue,
    // Informally, colorfulness / color intensity. Like saturation in HSL, except perceptually accurate.
    float Chroma,
    // Lightness
    float J,
    // >Brightness; ratio of lightness to white point's lightness
    float Q,
    // Colorfulness
    float M,
    // Saturation; ratio of chroma to white point's chroma
    float S,
    // CAM16-UCS J coordinate
    float Jstar,
    // CAM16-UCS a coordinate
    float Astar,
    // CAM16-UCS b coordinate
    float Bstar
)
{
    // Transformation matrices as static constants
    public readonly static Matrix3x3 XyzToConeMatrix = new(
        0.401288f,
        0.650173f,
        -0.051461f,
        -0.250268f,
        1.204414f,
        0.045854f,
        -0.002079f,
        0.048952f,
        0.953127f
    );

    public readonly static Matrix3x3 ConeToXyzMatrix = new(
        1.86206786f,
        -1.01125463f,
        0.14918677f,
        0.38752654f,
        0.62144744f,
        -0.00897398f,
        -0.01584150f,
        -0.03412294f,
        1.04996444f
    );

    /// <summary>
    /// CAM16 instances also have coordinates in the CAM16-UCS space, called J*,
    /// a*, b*, or jstar, astar, bstar in code. CAM16-UCS is included in the CAM16
    /// specification, and should be used when measuring distances between colors.
    /// </summary>
    public float Distance(Cam16 other)
    {
        var dJ = Jstar - other.Jstar;
        var dA = Astar - other.Astar;
        var dB = Bstar - other.Bstar;
        var dEPrime = MathF.Sqrt(dJ * dJ + dA * dA + dB * dB);
        var dE = 1.41f * MathF.Pow(dEPrime, 0.63f);
        return dE;
    }

    /// <summary>
    /// Convert ARGB to CAM16, assuming the color was viewed in default viewing conditions.
    /// </summary>
    public static Cam16 FromArgb(ArgbColor argb)
    {
        return FromArgbInViewingConditions(argb, ViewingConditions.SRgb);
    }

    /// <summary>
    /// Given viewing conditions, convert ARGB to CAM16.
    /// </summary>
    public static Cam16 FromArgbInViewingConditions(
        ArgbColor argb,
        ViewingConditions viewingConditions
    )
    {
        // Transform ARGB to XYZ
        var xyz = ColorUtils.XyzFromArgb(argb);
        return FromXyzInViewingConditions(xyz, viewingConditions);
    }

    /// <summary>
    /// Given color expressed in XYZ and viewed in viewing conditions, convert to CAM16.
    /// </summary>
    public static Cam16 FromXyzInViewingConditions(
        Vector3 xyz,
        ViewingConditions viewingConditions
    )
    {
        // Transform XYZ to 'cone'/'rgb' responses
        var coneResponse = XyzToConeMatrix * xyz;

        // Discount illuminant
        var discountedResponse = coneResponse * viewingConditions.RgbD;

        // Chromatic adaptation - vectorized operations
        var adaptationFactors = MathUtils.Pow(MathUtils.Abs(discountedResponse) * viewingConditions.Fl / 100.0f, 0.42f);
        var adaptedResponse =
            MathUtils.Signum(discountedResponse) * 400.0f * adaptationFactors / (adaptationFactors + new Vector3(27.13f));

        // Redness-greenness and yellowness-blueness
        var a = (11.0f * adaptedResponse.X + -12.0f * adaptedResponse.Y + adaptedResponse.Z) / 11.0f;
        var b = (adaptedResponse.X + adaptedResponse.Y - 2.0f * adaptedResponse.Z) / 9.0f;

        // Auxiliary components
        var u =
            (20.0f * adaptedResponse.X + 20.0f * adaptedResponse.Y + 21.0f * adaptedResponse.Z) / 20.0f;
        var p2 = (40.0f * adaptedResponse.X + 20.0f * adaptedResponse.Y + adaptedResponse.Z) / 20.0f;

        // Hue
        var atan2 = MathF.Atan2(b, a);
        var atanDegrees = atan2 * 180.0f / MathF.PI;
        var hue = MathUtils.SanitizeDegrees(atanDegrees);
        var hueRadians = hue * MathF.PI / 180.0f;
        System.Diagnostics.Debug.Assert(hue >= 0 && hue < 360, $"hue was really {hue}");

        // Achromatic response to color
        var ac = p2 * viewingConditions.Nbb;

        // CAM16 lightness and brightness
        var J =
            100.0f * MathF.Pow(ac / viewingConditions.Aw, viewingConditions.C * viewingConditions.Z);
        var Q =
            4.0f
            / viewingConditions.C
            * MathF.Sqrt(J / 100.0f)
            * (viewingConditions.Aw + 4.0f)
            * viewingConditions.FLRoot;

        var huePrime = hue < 20.14f ? hue + 360.0f : hue;
        var eHue = 1.0f / 4.0f * (MathF.Cos(huePrime * MathF.PI / 180.0f + 2.0f) + 3.8f);
        var p1 = 50000.0f / 13.0f * eHue * viewingConditions.NC * viewingConditions.Ncb;
        var t = p1 * MathF.Sqrt(a * a + b * b) / (u + 0.305f);
        var alpha =
            MathF.Pow(t, 0.9f)
            * MathF.Pow(1.64f - MathF.Pow(0.29f, viewingConditions.BackgroundYToWhitePointY), 0.73f);

        // CAM16 chroma, colorfulness, saturation
        var C = alpha * MathF.Sqrt(J / 100.0f);
        var M = C * viewingConditions.FLRoot;
        var s = 50.0f * MathF.Sqrt(alpha * viewingConditions.C / (viewingConditions.Aw + 4.0f));

        // CAM16-UCS components
        var jstar = (1.0f + 100.0f * 0.007f) * J / (1.0f + 0.007f * J);
        var mstar = MathF.Log(1.0f + 0.0228f * M) / 0.0228f;
        var astar = mstar * MathF.Cos(hueRadians);
        var bstar = mstar * MathF.Sin(hueRadians);

        return new Cam16(hue, C, J, Q, M, s, jstar, astar, bstar);
    }

    /// <summary>
    /// Create a CAM16 color from lightness J, chroma C, and hue H,
    /// assuming the color was viewed in default viewing conditions.
    /// </summary>
    public static Cam16 FromJch(float j, float c, float h)
    {
        return FromJchInViewingConditions(j, c, h, ViewingConditions.SRgb);
    }

    /// <summary>
    /// Create a CAM16 color from lightness J, chroma C, and hue H,
    /// in viewing conditions.
    /// </summary>
    public static Cam16 FromJchInViewingConditions(
        float J,
        float C,
        float h,
        ViewingConditions viewingConditions
    )
    {
        var Q =
            4.0f
            / viewingConditions.C
            * MathF.Sqrt(J / 100.0f)
            * (viewingConditions.Aw + 4.0f)
            * viewingConditions.FLRoot;
        var M = C * viewingConditions.FLRoot;
        var alpha = C / MathF.Sqrt(J / 100.0f);
        var s = 50.0f * MathF.Sqrt(alpha * viewingConditions.C / (viewingConditions.Aw + 4.0f));

        var hueRadians = h * MathF.PI / 180.0f;
        var jstar = (1.0f + 100.0f * 0.007f) * J / (1.0f + 0.007f * J);
        var mstar = 1.0f / 0.0228f * MathF.Log(1.0f + 0.0228f * M);
        var astar = mstar * MathF.Cos(hueRadians);
        var bstar = mstar * MathF.Sin(hueRadians);

        return new Cam16(h, C, J, Q, M, s, jstar, astar, bstar);
    }

    /// <summary>
    /// Create a CAM16 color from CAM16-UCS coordinates jstar, astar, bstar,
    /// assuming the color was viewed in default viewing conditions.
    /// </summary>
    public static Cam16 FromUcs(float jstar, float astar, float bstar)
    {
        return FromUcsInViewingConditions(jstar, astar, bstar, ViewingConditions.Standard);
    }

    /// <summary>
    /// Create a CAM16 color from CAM16-UCS coordinates jstar, astar, bstar,
    /// in viewing conditions.
    /// </summary>
    public static Cam16 FromUcsInViewingConditions(
        float jstar,
        float astar,
        float bstar,
        ViewingConditions viewingConditions
    )
    {
        var a = astar;
        var b = bstar;
        var m = MathF.Sqrt(a * a + b * b);
        var M = (MathF.Exp(m * 0.0228f) - 1.0f) / 0.0228f;
        var c = M / viewingConditions.FLRoot;
        var h = MathF.Atan2(b, a) * (180.0f / MathF.PI);
        if (h < 0.0f)
            h += 360.0f;
        var j = jstar / (1.0f - (jstar - 100.0f) * 0.007f);

        return FromJchInViewingConditions(j, c, h, viewingConditions);
    }

    /// <summary>
    /// ARGB representation of color, assuming the color was viewed in default viewing conditions.
    /// </summary>
    public ArgbColor ToArgb()
    {
        return ViewedInConditions(ViewingConditions.SRgb);
    }

    /// <summary>
    /// ARGB representation of a color, given the color was viewed in viewing conditions.
    /// </summary>
    public ArgbColor ViewedInConditions(ViewingConditions viewingConditions)
    {
        var xyz = XyzInViewingConditions(viewingConditions);
        return ColorUtils.ArgbFromXyz(new Vector3((float)xyz.X, (float)xyz.Y, (float)xyz.Z));
    }

    /// <summary>
    /// XYZ representation of CAM16 seen in viewing conditions.
    /// </summary>
    public Vector3 XyzInViewingConditions(ViewingConditions viewingConditions)
    {
        var alpha = Chroma == 0.0f || J == 0.0f ? 0.0f : Chroma / MathF.Sqrt(J / 100.0f);

        var t = MathF.Pow(
            alpha
            / MathF.Pow(1.64f - MathF.Pow(0.29f, viewingConditions.BackgroundYToWhitePointY), 0.73f),
            1.0f / 0.9f
        );
        var hRad = Hue * MathF.PI / 180.0f;

        var eHue = 0.25f * (MathF.Cos(hRad + 2.0f) + 3.8f);
        var ac =
            viewingConditions.Aw
            * MathF.Pow(J / 100.0f, 1.0f / viewingConditions.C / viewingConditions.Z);
        var p1 = eHue * (50000.0f / 13.0f) * viewingConditions.NC * viewingConditions.Ncb;

        var p2 = ac / viewingConditions.Nbb;

        var hSin = MathF.Sin(hRad);
        var hCos = MathF.Cos(hRad);

        var gamma = 23.0f * (p2 + 0.305f) * t / (23.0f * p1 + 11.0f * t * hCos + 108.0f * t * hSin);
        var a = gamma * hCos;
        var b = gamma * hSin;

        var p2Vector = new Vector3(460.0f * p2, 460.0f * p2, 460.0f * p2);
        var coefficientsA = new Vector3(451.0f * a, -891.0f * a, -220.0f * a);
        var coefficientsB = new Vector3(288.0f * b, -261.0f * b, -6300.0f * b);
        var rgbA = (p2Vector + coefficientsA + coefficientsB) / 1403.0f;

        var absRgbA = MathUtils.Abs(rgbA);
        var rgbCBase = MathUtils.Max(absRgbA * 27.13f / (new Vector3(400.0f) - absRgbA), 0.0f);
        var rgbC = MathUtils.Signum(rgbA) * (100.0f / viewingConditions.Fl) * MathUtils.Pow(rgbCBase, 1.0f / 0.42f);
        var rgbF = rgbC / viewingConditions.RgbD;

        var xyz = ConeToXyzMatrix * rgbF;

        return xyz;
    }
}
