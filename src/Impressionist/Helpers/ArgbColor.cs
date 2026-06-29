using System.Numerics;

namespace Impressionist.Helpers;

/// <summary>
/// Represents a 32-bit ARGB (alpha, red, green, blue) color value.
/// </summary>
/// <remarks>
/// The ArgbColor struct provides convenient access to the individual alpha, red, green, and blue
/// components of a color, as well as the packed 32-bit integer value. It has the same level of
/// efficiency as using a raw integer, while being significantly easier to use.
/// It is used to replace int-based color representations in the original library.
/// </remarks>
public struct ArgbColor : IEquatable<ArgbColor>
{
    private int _value;
    public int Value => _value;

    public ArgbColor(int value)
    {
        _value = value;
    }

    public ArgbColor(uint value)
    {
        _value = unchecked((int)value);
    }

    public ArgbColor(byte alpha, byte red, byte green, byte blue)
    {
        _value = (alpha << 24) | (red << 16) | (green << 8) | blue;
    }

    public ArgbColor(Vector3 v) : this(255, (byte)v.X, (byte)v.Y, (byte)v.Z)
    {
    }

    public ArgbColor(Vector4 v) : this((byte)v.X, (byte)v.Y, (byte)v.Z, (byte)v.W)
    {
    }

    public void Deconstruct(out byte alpha, out byte red, out byte green, out byte blue)
    {
        alpha = Alpha;
        red = Red;
        green = Green;
        blue = Blue;
    }

    public byte Alpha
    {
        get => (byte)((_value >> 24) & 0xFF);
        set => _value = (_value & 0x00FFFFFF) | ((value & 0xFF) << 24);
    }

    public byte Red
    {
        get => (byte)((_value >> 16) & 0xFF);
        set => _value = (int)(_value & 0xFF00FFFF) | ((value & 0xFF) << 16);
    }

    public byte Green
    {
        get => (byte)((_value >> 8) & 0xFF);
        set => _value = (int)(_value & 0xFFFF00FF) | ((value & 0xFF) << 8);
    }

    public byte Blue
    {
        get => (byte)(_value & 0xFF);
        set => _value = (int)(_value & 0xFFFFFF00) | (value & 0xFF);
    }

    public bool IsOpaque()
    {
        return Alpha == 255;
    }

    public bool Equals(ArgbColor other)
    {
        return _value == other._value;
    }

    override public bool Equals(object? obj)
    {
        return obj is ArgbColor other && Equals(other);
    }

    override public int GetHashCode()
    {
        return _value;
    }

    public static bool operator ==(ArgbColor left, ArgbColor right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ArgbColor left, ArgbColor right)
    {
        return !(left == right);
    }

    public static ArgbColor operator +(ArgbColor left, ArgbColor right)
    {
        return new ArgbColor(
            (byte)(left.Alpha + right.Alpha),
            (byte)(left.Red + right.Red),
            (byte)(left.Green + right.Green),
            (byte)(left.Blue + right.Blue));
    }

    public static ArgbColor operator /(ArgbColor color, int divisor)
    {
        return new ArgbColor(
            (byte)(color.Alpha / divisor),
            (byte)(color.Red / divisor),
            (byte)(color.Green / divisor),
            (byte)(color.Blue / divisor));
    }

    override public string ToString()
    {
        return $"#{Value:X8}";
    }

    public static int DistanceSquared(ArgbColor left, ArgbColor right)
    {
        return (left.Alpha - right.Alpha) * (left.Alpha - right.Alpha) +
               (left.Red - right.Red) * (left.Red - right.Red) +
               (left.Green - right.Green) * (left.Green - right.Green) +
               (left.Blue - right.Blue) * (left.Blue - right.Blue);
    }

    public static ArgbColor Zero = new ArgbColor(0, 0, 0, 0);
}
