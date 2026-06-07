using System.Numerics;

namespace Cirrus.Network.Extensions;

/// <summary>
/// Converter extensions between <see cref="BigInteger"/> and <see cref="byte"/> array,
/// used by RSA encryptors.
/// </summary>
internal static class BigIntegerExtension
{
    /// <summary>
    /// Convert the <see cref="byte"/> array to <see cref="BigInteger"/>.
    /// </summary>
    /// <param name="buffer">The <see cref="byte"/> array to be converted.</param>
    /// <returns>The converted <see cref="BigInteger"/>.</returns>
    public static BigInteger ToBigInteger(this byte[] buffer)
    {
        var extendedBuffer = new byte[buffer.Length + 1];
        buffer.Reverse().ToArray().CopyTo(extendedBuffer, 0);
        return new(extendedBuffer);
    }

    /// <summary>
    /// Convert the <see cref="BigInteger"/> to <see cref="byte"/> array.
    /// </summary>
    /// <param name="bigInteger">The <see cref="BigInteger"/> to be converted.</param>
    /// <returns>The converted <see cref="byte"/> array.</returns>
    public static byte[] ToBytes(this BigInteger bigInteger)
    {
        var reversedBuffer = bigInteger.ToByteArray().Reverse().ToArray();
        return reversedBuffer[0] == 0 ? reversedBuffer.Skip(1).ToArray() : reversedBuffer;
    }
}