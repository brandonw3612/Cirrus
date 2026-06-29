using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Impressionist.Helpers.Hct;
/// <summary>
/// In traditional color spaces, a color can be identified solely by the
/// observer's measurement of the color. Color appearance models such as CAM16
/// also use information about the environment where the color was
/// observed, known as the viewing conditions.
///
/// For example, white under the traditional assumption of a midday sun white
/// point is accurately measured as a slightly chromatic blue by CAM16.
/// (roughly, hue 203, chroma 3, lightness 100)
///
/// This struct caches intermediate values of the CAM16 conversion process that
/// depend only on viewing conditions, enabling speed ups.
/// </summary>
public readonly record struct ViewingConditions(
    Vector3 WhitePoint,
    float AdaptingLuminance,
    float BackgroundLstar,
    float Surround,
    bool DiscountingIlluminant,
    float BackgroundYToWhitePointY,
    float Aw,
    float Nbb,
    float Ncb,
    float C,
    float NC,
    Vector3 DrgbInverse,
    Vector3 RgbD,
    float Fl,
    float FLRoot,
    float Z
)
{
    // XYZ to cone response transformation matrix (same as in Cam16)
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

    readonly private static ViewingConditions _standard = Make();

    // private static readonly ViewingConditions _srgb = Make();

    public static ref readonly ViewingConditions Standard => ref _standard;
    public static ref readonly ViewingConditions SRgb => ref _standard;

    /// <summary>
    /// Convenience factory method for ViewingConditions.
    ///
    /// Parameters affecting color appearance include:
    /// </summary>
    /// <param name="whitePoint">Coordinates of white in XYZ color space. If null, uses D65 white point.</param>
    /// <param name="adaptingLuminance">Light strength, in lux. If negative, defaults calculated value.</param>
    /// <param name="backgroundLstar">Average luminance of 10 degrees around color.</param>
    /// <param name="surround">Brightness of the entire environment.</param>
    /// <param name="discountingIlluminant">Whether eyes have adjusted to lighting.</param>
    /// <returns>ViewingConditions with the specified parameters</returns>
    public static ViewingConditions Make(
        Vector3? whitePoint = null,
        float adaptingLuminance = -1.0f,
        float backgroundLstar = 50.0f,
        float surround = 2.0f,
        bool discountingIlluminant = false
    )
    {
        var wp = whitePoint ?? ColorUtils.WhitePointD65;

        adaptingLuminance =
            adaptingLuminance > 0.0f
                ? adaptingLuminance
                : 200.0f / MathF.PI * ColorUtils.YFromLstar(50.0f) / 100.0f;

        // A background of pure black is non-physical and leads to infinities that
        // represent the idea that any color viewed in pure black can't be seen.
        backgroundLstar = MathF.Max(0.1f, backgroundLstar);

        // Transform test illuminant white in XYZ to 'cone'/'rgb' responses
        var whitePointConeResponse = XyzToConeMatrix * wp;

        // Scale input surround, domain (0, 2), to CAM16 surround, domain (0.8, 1.0)
        System.Diagnostics.Debug.Assert(surround >= 0.0f && surround <= 2.0f);
        var f = 0.8f + surround / 10.0f;

        // "Exponential non-linearity"
        var c =
            f >= 0.9f
                ? MathUtils.Lerp(0.59f, 0.69f, (f - 0.9f) * 10.0f)
                : MathUtils.Lerp(0.525f, 0.59f, (f - 0.8f) * 10.0f);

        // Calculate degree of adaptation to illuminant
        var d = discountingIlluminant
            ? 1.0f
            : f * (1.0f - 1.0f / 3.6f * MathF.Exp((-adaptingLuminance - 42.0f) / 92.0f));

        // Per Li et al, if D is greater than 1 or less than 0, set it to 1 or 0.
        d = Math.Clamp(d, 0.0f, 1.0f);

        // chromatic induction factor
        var nc = f;

        // Cone responses to the whitePoint, r/g/b/W, adjusted for discounting
        //
        // Why use 100.0 instead of the white point's relative luminance?
        //
        // Some papers and implementations, for both CAM02 and CAM16, use the Y
        // value of the reference white instead of 100. Fairchild's Color Appearance
        // Models (3rd edition) notes that this is in error: it was included in the
        // CIE 2004a report on CIECAM02, but, later parts of the conversion process
        // account for scaling of appearance relative to the white point relative
        // luminance. This part should simply use 100 as luminance.
        var rgbD = d * (new Vector3(100.0f, 100.0f, 100.0f) / whitePointConeResponse) + new Vector3(1.0f - d);

        // Factor used in calculating meaningful factors
        var k = 1.0f / (5.0f * adaptingLuminance + 1.0f);
        var k4 = k * k * k * k;
        var k4F = 1.0f - k4;

        // Luminance-level adaptation factor
        var fl =
            k4 * adaptingLuminance + 0.1f * k4F * k4F * MathF.Pow(5.0f * adaptingLuminance, 1.0f / 3.0f);

        // Intermediate factor, ratio of background relative luminance to white relative luminance
        var n = ColorUtils.YFromLstar(backgroundLstar) / wp.Y;

        // Base exponential nonlinearity
        // note Schlomer 2018 has a typo and uses 1.58, the correct factor is 1.48
        var z = 1.48f + MathF.Sqrt(n);

        // Luminance-level induction factors
        var nbb = 0.725f / MathF.Pow(n, 0.2f);
        var ncb = nbb;

        // Discounted cone responses to the white point, adjusted for post-adaptation
        // perceptual nonlinearities - vectorized
        var rgbAFactors = MathUtils.Pow(fl * rgbD * whitePointConeResponse / 100.0f, 0.42f);
        var rgbA = 400.0f * rgbAFactors / (rgbAFactors + new Vector3(27.13f));

        var aw = (40.0f * rgbA.X + 20.0f * rgbA.Y + rgbA.Z) / 20.0f * nbb;

        return new ViewingConditions(
            wp,
            adaptingLuminance,
            backgroundLstar,
            surround,
            discountingIlluminant,
            n,
            aw,
            nbb,
            ncb,
            c,
            nc,
            Vector3.Zero,
            rgbD,
            fl,
            MathF.Pow(fl, 0.25f),
            z
        );
    }
}