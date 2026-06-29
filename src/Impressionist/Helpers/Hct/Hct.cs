using System;
using System.Collections.Generic;
using System.Text;

namespace Impressionist.Helpers.Hct;
/// <summary>
/// HCT, hue, chroma, and tone. A color system that provides a perceptually
/// accurate color measurement system that can also accurately render what
/// colors will appear as in different lighting environments.
/// </summary>
public struct Hct : IEquatable<Hct>
{
    private float _hue;
    private float _chroma;
    private float _tone;
    private ArgbColor _argb;

    private Hct(ArgbColor argb)
    {
        _argb = argb;
        var cam16 = Cam16.FromArgb(argb);
        _hue = cam16.Hue;
        _chroma = cam16.Chroma;
        _tone = ColorUtils.LstarFromArgb(_argb);
    }

    public void Deconstruct(out float hue, out float chroma, out float tone)
    {
        hue = _hue;
        chroma = _chroma;
        tone = _tone;
    }

    /// <param name="hue">
    /// 0 &lt;= Hue &lt; 360;
    ///     <para>
    ///     invalid values are corrected.
    ///     </para>
    /// </param>
    /// <param name="chroma">
    /// 0 &lt;= chroma;
    ///     <para>
    ///     Informally, colorfulness. The color returned may be
    ///     lower than the requested chroma. Chroma has a different maximum for any
    ///     given hue and tone.
    ///     </para>
    /// </param>
    /// <param name="tone">
    /// 0 &lt;= tone &lt; 100;
    ///     <para>
    ///     informally, lightness. Invalid values are corrected.
    ///     </para>
    /// </param>
    public static Hct From(float hue, float chroma, float tone)
    {
        var argb = HctSolver.SolveToArgb(hue, chroma, tone);
        return new Hct(argb);
    }

    /// <summary>
    /// HCT representation of the given ARGB color.
    /// </summary>
    /// <param name="argb">ARGB color to convert to HCT.</param>
    public static Hct From(ArgbColor argb)
    {
        return new Hct(argb);
    }

    /// <summary>
    /// ARGB representation of this HCT color.
    /// </summary>
    public ArgbColor Argb => _argb;

    /// <summary>
    /// A number, in degrees, representing ex. red, orange, yellow, etc.
    /// Ranges from 0 &lt;= Hue &lt; 360.
    /// <para>
    /// Invalid values are corrected. After setting hue, the color is mapped from
    /// HCT to the more limited sRGB gamut for display. This will change its ARGB
    /// representation. If the HCT color is outside the sRGB gamut, chroma will
    /// decrease until it is inside the gamut.
    /// </para>
    /// </summary>
    public float Hue
    {
        get => _hue;
        set
        {
            _argb = HctSolver.SolveToArgb(value, _chroma, _tone);
            var cam16 = Cam16.FromArgb(_argb);
            _hue = cam16.Hue;
            _chroma = cam16.Chroma;
            _tone = ColorUtils.LstarFromArgb(_argb);
        }
    }

    /// <summary>
    /// Informally, colorfulness. The color returned may be lower than the
    /// requested chroma. Chroma has a different maximum for any given hue and
    /// tone. After setting chroma, the color is mapped from HCT to the more
    /// limited sRGB gamut for display. This will change its ARGB
    /// representation. If the HCT color is outside the sRGB gamut, chroma
    /// will decrease until it is inside the gamut.
    /// </summary>
    public float Chroma
    {
        get => _chroma;
        set
        {
            _argb = HctSolver.SolveToArgb(_hue, value, _tone);
            var cam16 = Cam16.FromArgb(_argb);
            _hue = cam16.Hue;
            _chroma = cam16.Chroma;
            _tone = ColorUtils.LstarFromArgb(_argb);
        }
    }

    /// <summary>
    /// Lightness. Ranges from 0 to 100.
    /// <para>
    /// After setting tone, the color is mapped from HCT to the more limited
    /// sRGB gamut for display. This will change its ARGB representation. If the
    /// HCT color is outside the sRGB gamut, chroma will decrease until it is
    /// inside the gamut.
    /// </para>
    /// </summary>
    public float Tone
    {
        get => _tone;
        set
        {
            _argb = HctSolver.SolveToArgb(_hue, _chroma, value);
            var cam16 = Cam16.FromArgb(_argb);
            _hue = cam16.Hue;
            _chroma = cam16.Chroma;
            _tone = ColorUtils.LstarFromArgb(_argb);
        }
    }

    public ArgbColor ToArgb() => Argb;

    override public int GetHashCode()
    {
        return HashCode.Combine(_argb);
    }

    override public bool Equals(object? obj)
    {
        return obj is Hct other && _argb.Equals(other._argb);
    }

    public bool Equals(Hct other)
    {
        return _argb.Equals(other._argb);
    }

    override public string ToString()
    {
        return $"HCT: {_hue:F1}°, {_chroma:F1}, {_tone:F1} (ARGB: {_argb.Value:X8})";
    }

    /// <summary>
    /// Translate a color into different viewing conditions.
    ///
    /// Colors change appearance. They look different with lights on versus off,
    /// the same color, as in hex code, on white looks different when on black.
    /// This is called color relativity, most famously explicated by Josef Albers
    /// in Interaction of Color.
    ///
    /// In color science, color appearance models can account for this and
    /// calculate the appearance of a color in different settings. HCT is based on
    /// CAM16, a color appearance model, and uses it to make these calculations.
    ///
    /// See <see cref="ViewingConditions.Make"/> for parameters affecting color appearance.
    /// </summary>
    /// <param name="vc">Viewing conditions to translate this color into.</param>
    public Hct InViewingConditions(ViewingConditions vc)
    {
        // 1. Use CAM16 to find XYZ coordinates of color in specified VC.
        var cam16 = Cam16.FromArgb(Argb);
        var viewedInVc = cam16.XyzInViewingConditions(vc);

        // 2. Create CAM16 of those XYZ coordinates in default VC.
        var recastInVc = Cam16.FromXyzInViewingConditions(viewedInVc, ViewingConditions.Make());

        // 3. Create HCT from:
        // - CAM16 using default VC with XYZ coordinates in specified VC.
        // - L* converted from Y in XYZ coordinates in specified VC.
        var recastHct = From(
            recastInVc.Hue,
            recastInVc.Chroma,
            ColorUtils.LstarFromY(viewedInVc[1])
        );

        return recastHct;
    }
    public static bool operator ==(Hct left, Hct right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Hct left, Hct right)
    {
        return !(left == right);
    }

    public static bool IsBlue(float hue)
    {
        return hue >= 250 && hue < 270;
    }

    public static bool IsYellow(float hue)
    {
        return hue >= 105 && hue < 125;
    }

    public static bool IsCyan(float hue)
    {
        return hue >= 170 && hue < 207;
    }
}
