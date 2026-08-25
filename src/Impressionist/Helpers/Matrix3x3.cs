using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Impressionist.Helpers;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Matrix3x3 : IEquatable<Matrix3x3>
{
    public readonly float M11, M12, M13;
    public readonly float M21, M22, M23;
    public readonly float M31, M32, M33;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix3x3(
        float m11, float m12, float m13,
        float m21, float m22, float m23,
        float m31, float m32, float m33)
    {
        M11 = m11;
        M12 = m12;
        M13 = m13;
        M21 = m21;
        M22 = m22;
        M23 = m23;
        M31 = m31;
        M32 = m32;
        M33 = m33;
    }

    public static Matrix3x3 Identity => new(
        1.0f, 0.0f, 0.0f,
        0.0f, 1.0f, 0.0f,
        0.0f, 0.0f, 1.0f);

    public static Matrix3x3 Zero => new(
        0.0f, 0.0f, 0.0f,
        0.0f, 0.0f, 0.0f,
        0.0f, 0.0f, 0.0f);

    public float this[int row, int col]
    {
        get
        {
            return (row, col) switch
            {
                (0, 0) => M11,
                (0, 1) => M12,
                (0, 2) => M13,
                (1, 0) => M21,
                (1, 1) => M22,
                (1, 2) => M23,
                (2, 0) => M31,
                (2, 1) => M32,
                (2, 2) => M33,
                _ => throw new IndexOutOfRangeException()
            };
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix3x3 operator +(in Matrix3x3 a, in Matrix3x3 b)
    {
        return new Matrix3x3(
            a.M11 + b.M11, a.M12 + b.M12, a.M13 + b.M13,
            a.M21 + b.M21, a.M22 + b.M22, a.M23 + b.M23,
            a.M31 + b.M31, a.M32 + b.M32, a.M33 + b.M33);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix3x3 operator -(in Matrix3x3 a, in Matrix3x3 b)
    {
        return new Matrix3x3(
            a.M11 - b.M11, a.M12 - b.M12, a.M13 - b.M13,
            a.M21 - b.M21, a.M22 - b.M22, a.M23 - b.M23,
            a.M31 - b.M31, a.M32 - b.M32, a.M33 - b.M33);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix3x3 operator *(in Matrix3x3 a, float s)
    {
        return new Matrix3x3(
            a.M11 * s, a.M12 * s, a.M13 * s,
            a.M21 * s, a.M22 * s, a.M23 * s,
            a.M31 * s, a.M32 * s, a.M33 * s);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix3x3 operator *(float s, in Matrix3x3 a)
    {
        return a * s;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix3x3 operator *(in Matrix3x3 a, in Matrix3x3 b)
    {
        return new Matrix3x3(
            a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31,
            a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32,
            a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33,
            a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31,
            a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32,
            a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33,
            a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31,
            a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32,
            a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 operator *(in Matrix3x3 m, in Vector3 v)
    {
        return new Vector3(
            m.M11 * v.X + m.M12 * v.Y + m.M13 * v.Z,
            m.M21 * v.X + m.M22 * v.Y + m.M23 * v.Z,
            m.M31 * v.X + m.M32 * v.Y + m.M33 * v.Z);
    }

    override public string ToString()
    {
        return $"[{M11}, {M12}, {M13}; {M21}, {M22}, {M23}; {M31}, {M32}, {M33}]";
    }

    public bool Equals(Matrix3x3 other)
    {
        return M11 == other.M11 && M12 == other.M12 && M13 == other.M13 &&
               M21 == other.M21 && M22 == other.M22 && M23 == other.M23 &&
               M31 == other.M31 && M32 == other.M32 && M33 == other.M33;
    }

    override public bool Equals(object? obj)
    {
        return obj is Matrix3x3 m && Equals(m);
    }

    override public int GetHashCode()
    {
        return HashCode.Combine(HashCode.Combine(M11, M12), M13, M21, M22, M23, M31, M32, M33);
    }
}