using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Impressionist.Helpers;
/// <summary>
/// Mathematical utility functions for color calculations.
/// </summary>
public static class MathUtils
{
    /// <summary>
    /// Returns the sign of a number: -1 for negative, 0 for zero, 1 for positive.
    /// </summary>
    /// <param name="value">The value to get the sign of</param>
    /// <returns>-1, 0, or 1</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float Signum(float value)
    {
        return value < 0.0f ? -1.0f
            : value > 0.0f ? 1.0f
            : 0.0f;
    }

    /// <summary>
    /// Linearly interpolates between two values.
    /// </summary>
    /// <param name="start">Start value</param>
    /// <param name="end">End value</param>
    /// <param name="amount">Interpolation factor (0.0 to 1.0)</param>
    /// <returns>Interpolated value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float Lerp(float start, float end, float amount)
    {
        return start + (end - start) * amount;
    }

    /// <summary>
    /// Sanitizes degrees to be within the range [0, 360).
    /// </summary>
    /// <typeparam name="T">Numeric type that supports INumber interface.</typeparam>
    /// <param name="degrees">Angle in degrees.</param>
    /// <returns>Normalized angle in [0, 360) range.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T SanitizeDegrees<T>(T degrees)
        where T : INumber<T>
    {
        var fullCircle = T.CreateChecked(360.0f);
        var result = degrees % fullCircle;
        if (result < T.Zero)
            result += fullCircle;
        return result;
    }

    /// <summary>
    /// Returns the direction of rotation from one angle to another.
    /// </summary>
    /// <param name="from">Starting angle in degrees.</param>
    /// <param name="to">Ending angle in degrees.</param>
    /// <returns>1.0 for clockwise rotation, -1.0 for counter-clockwise.</returns>
    internal static float RotationDirection(float from, float to)
    {
        var increasingDifference = SanitizeDegrees(to - from);
        return increasingDifference <= 180.0f ? 1.0f : -1.0f;
    }

    /// <summary>
    /// Returns the difference between two angles in degrees.
    /// The result is always in the range [0, 180].
    /// </summary>
    /// <param name="a">First angle in degrees.</param>
    /// <param name="b">Second angle in degrees.</param>
    /// <returns>Difference in degrees [0, 180].</returns>
    internal static float DifferenceDegrees(float a, float b)
    {
        return 180.0f - MathF.Abs(MathF.Abs(a - b) - 180.0f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector3 Abs(Vector3 value)
    {
        return Vector3.Abs(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector3 Signum(Vector3 value)
    {
        return new Vector3(Signum(value.X), Signum(value.Y), Signum(value.Z));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector3 Pow(Vector3 value, float power)
    {
        return new Vector3(
            MathF.Pow(value.X, power),
            MathF.Pow(value.Y, power),
            MathF.Pow(value.Z, power));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector3 Max(Vector3 value, float min)
    {
        return Vector3.Max(value, new Vector3(min, min, min));
    }
}