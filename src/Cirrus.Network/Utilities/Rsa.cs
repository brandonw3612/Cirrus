using System.Numerics;
using Cirrus.Network.Extensions;

namespace Cirrus.Network.Utilities;

/// <summary>
/// RSA algorithm implementation without padding.
/// </summary>
internal sealed class Rsa
{
    private readonly BigInteger _exponent;
    private readonly BigInteger _modulus;

    /// <summary>
    /// Creates an RSA cryptographic object with public key, in specific, exponent and modulus, in bytes
    /// </summary>
    /// <param name="exponent">Exponent from the public key.</param>
    /// <param name="modulus">Modulus from the public key.</param>
    public Rsa(byte[] exponent, byte[] modulus)
    {
        _exponent = exponent.ToBigInteger();
        _modulus = modulus.ToBigInteger();
    }

    /// <summary>
    /// Encrypt data buffer with the algorithm.
    /// </summary>
    /// <param name="buffer">Original buffer to be encrypted.</param>
    /// <returns>Buffer encrypted by the algorithm.</returns>
    public byte[] Encrypt(byte[] buffer)
    {
        var value = buffer.ToBigInteger();
        return BigInteger.ModPow(value, _exponent, _modulus).ToBytes();
    }
}