using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Impressionist.Helpers;

public static class ColorUtils
{
    readonly private static Matrix3x3 SrgbToXyz = new(
        0.41233895f,
        0.35762064f,
        0.18051042f,
        0.2126f,
        0.7152f,
        0.0722f,
        0.01932141f,
        0.11916382f,
        0.95034478f
    );

    readonly private static Matrix3x3 XyzToSrgb = new(
        3.2413774792388685f,
        -1.5376652402851851f,
        -0.49885366846268053f,
        -0.9691452513005321f,
        1.8758853451067872f,
        0.04156585616912061f,
        0.05562093689691305f,
        -0.20395524564742123f,
        1.0571799111220335f
    );

    /// <summary>
    /// The D65 standard illuminant white point in XYZ color space.
    /// </summary>
    internal readonly static Vector3 WhitePointD65 = new(95.047f, 100.0f, 108.883f);

    /// <summary>
    /// Converts RGB components to ARGB color.
    /// </summary>
    /// <param name="red">Red component (0-255).</param>
    /// <param name="green">Green component (0-255).</param>
    /// <param name="blue">Blue component (0-255).</param>
    /// <returns>ARGB color with full opacity.</returns>
    internal static ArgbColor ArgbFromRgb(int red, int green, int blue)
    {
        return new ArgbColor(255, (byte)red, (byte)green, (byte)blue);
    }

    /// <summary>
    /// Converts linear RGB to ARGB color.
    /// </summary>
    /// <param name="linrgb">Linear RGB values as a Vector3D.</param>
    /// <returns>ARGB color.</returns>
    internal static ArgbColor ArgbFromLinrgb(Vector3 linrgb)
    {
        return Delinearized(linrgb);
    }

    /// <summary>
    /// Converts XYZ color space values to ARGB color.
    /// </summary>
    /// <param name="xyz">XYZ values as a Vector3D.</param>
    /// <returns>ARGB color.</returns>
    internal static ArgbColor ArgbFromXyz(Vector3 xyz)
    {
        // Convert from XYZ to linear sRGB, then delinearize to bytes.
        ref readonly var matrix = ref XyzToSrgb;
        var linearRGB = matrix * xyz;
        return Delinearized(linearRGB);
    }

    /// <summary>
    /// Converts ARGB color to XYZ color space values.
    /// </summary>
    /// <param name="argb">ARGB color.</param>
    /// <returns>XYZ values as a Vector3D.</returns>
    internal static Vector3 XyzFromArgb(ArgbColor argb)
    {
        // Convert from ARGB bytes to linear sRGB, then to XYZ.
        var linearRGB = Linearized(argb);
        ref readonly var matrix = ref SrgbToXyz;
        return matrix * linearRGB;
    }

    /// <summary>
    /// Converts LAB color space values to ARGB color.
    /// </summary>
    /// <param name="lab">LAB values as a Vector3D (L, a, b).</param>
    /// <returns>ARGB color.</returns>
    internal static ArgbColor ArgbFromLab(Vector3 lab)
    {
        ref readonly var whitePoint = ref WhitePointD65;

        var fy = (lab.X + 16.0f) / 116.0f;
        var fx = lab.Y / 500.0f + fy;
        var fz = fy - lab.Z / 200.0f;

        var xyzNormalized = LabInvf(new Vector3(fx, fy, fz));
        return ArgbFromXyz(xyzNormalized * whitePoint);
    }

    /// <summary>
    /// Converts ARGB color to LAB color space values.
    /// </summary>
    /// <param name="argb">ARGB color.</param>
    /// <returns>LAB values as a Vector3D (L, a, b).</returns>
    internal static Vector3 LabFromArgb(ArgbColor argb)
    {
        ref readonly var whitePoint = ref WhitePointD65;
        ref readonly var matrix = ref SrgbToXyz;

        var linearRGB = Linearized(argb);
        var xyz = matrix * linearRGB;
        var xyzNormalized = xyz / whitePoint;
        var f = LabF(xyzNormalized);
        return new Vector3(116 * f.Y - 16, 500 * (f.X - f.Y), 200 * (f.Y - f.Z));
    }

    /// <summary>
    /// Converts L* (lightness) value to ARGB color (grayscale).
    /// </summary>
    /// <param name="lstar">L* value (0-100).</param>
    /// <returns>Grayscale ARGB color.</returns>
    internal static ArgbColor ArgbFromLstar(float lstar)
    {
        var y = YFromLstar(lstar);
        var component = Delinearized(y);
        return new ArgbColor(255, component, component, component);
    }

    /// <summary>
    /// Calculates L* (lightness) from ARGB color.
    /// </summary>
    /// <param name="argb">ARGB color.</param>
    /// <returns>L* value (0-100).</returns>
    internal static float LstarFromArgb(ArgbColor argb)
    {
        var y = XyzFromArgb(argb).Y;
        return 116.0f * LabF(y / 100.0f) - 16.0f;
    }

    /// <summary>
    /// Converts L* (lightness) to Y in XYZ color space.
    /// </summary>
    /// <param name="lstar">L* value (0-100).</param>
    /// <returns>Y value in XYZ.</returns>
    internal static float YFromLstar(float lstar)
    {
        return 100.0f * LabInvf((lstar + 16.0f) / 116.0f);
    }

    /// <summary>
    /// Converts Y in XYZ color space to L* (lightness).
    /// </summary>
    /// <param name="y">Y value in XYZ.</param>
    /// <returns>L* value (0-100).</returns>
    internal static float LstarFromY(float y)
    {
        return LabF(y / 100.0f) * 116.0f - 16.0f;
    }

    internal static float Linearized(byte rgbComponent)
    {
        var normalized = rgbComponent / 255.0f;
        return normalized <= 0.040449936f
            ? (normalized / 12.92f * 100.0f)
            : (MathF.Pow((normalized + 0.055f) / 1.055f, 2.4f) * 100.0f);
    }

    internal static Vector3 Linearized(ArgbColor argb)
    {
        return new Vector3(Linearized(argb.Red), Linearized(argb.Green), Linearized(argb.Blue));
    }

    internal static byte Delinearized(float linearComponent)
    {
        var normalized = linearComponent / 100.0f;
        var delinearized =
            normalized <= 0.0031308f
                ? normalized * 12.92f
                : 1.055f * MathF.Pow(normalized, 1.0f / 2.4f) - 0.055f;
        return (byte)
            Math.Clamp(MathF.Round(delinearized * 255.0f, MidpointRounding.AwayFromZero), 0.0f, 255.0f);
    }

    internal static ArgbColor Delinearized(Vector3 linearRgb)
    {
        return new ArgbColor(
            255,
            Delinearized(linearRgb.X),
            Delinearized(linearRgb.Y),
            Delinearized(linearRgb.Z)
        );
    }

    private static Vector3 LabF(Vector3 t)
    {
        return new Vector3(LabF(t.X), LabF(t.Y), LabF(t.Z));
    }

    private static float LabF(float t)
    {
        const float e = 216.0f / 24389.0f;
        const float kappa = 24389.0f / 27.0f;
        return t > e ? MathF.Pow(t, 1.0f / 3.0f) : (kappa * t + 16.0f) / 116.0f;
    }

    private static Vector3 LabInvf(Vector3 ft)
    {
        return new Vector3(LabInvf(ft.X), LabInvf(ft.Y), LabInvf(ft.Z));
    }

    private static float LabInvf(float ft)
    {
        const float e = 216.0f / 24389.0f;
        const float kappa = 24389.0f / 27.0f;
        var ft3 = ft * ft * ft;
        return ft3 > e ? ft3 : (116.0f * ft - 16.0f) / kappa;
    }
}
