using System.Security.Cryptography;

namespace Cirrus.Network.Utilities;

/// <summary>
/// Disposable AES wrapper with simplified constructor methods.
/// </summary>
internal class AesWrapper : IDisposable
{
    private Aes? _aes;

    /// <summary>
    /// Creates a cryptographic object that is used to perform the symmetric algorithm.
    /// </summary>
    /// <param name="blockSize">Block size, in bits, of the cryptographic operation.</param>
    /// <param name="cipherMode">Cipher mode for operation of the symmetric algorithm.</param>
    /// <param name="secretKey">Secret key for the symmetric algorithm.</param>
    public AesWrapper(int blockSize, CipherMode cipherMode, byte[] secretKey)
    {
        _aes = Aes.Create();
        _aes.BlockSize = blockSize;
        _aes.Mode = cipherMode;
        _aes.Key = secretKey;
    }
    
    /// <summary>
    /// Creates a cryptographic object that is used to perform the symmetric algorithm.
    /// </summary>
    /// <param name="blockSize">Block size, in bits, of the cryptographic operation.</param>
    /// <param name="cipherMode">Cipher mode for operation of the symmetric algorithm.</param>
    /// <param name="secretKey">Secret key for the symmetric algorithm.</param>
    /// <param name="initializationVector">Initialization vector (IV) for the symmetric algorithm.</param>
    public AesWrapper(int blockSize, CipherMode cipherMode, byte[] secretKey, byte[] initializationVector)
    {
        _aes = Aes.Create();
        _aes.BlockSize = blockSize;
        _aes.Mode = cipherMode;
        _aes.Key = secretKey;
        _aes.IV = initializationVector;
    }

    /// <summary>
    /// Encrypt data buffer with the algorithm.
    /// </summary>
    /// <param name="buffer">Original buffer to be encrypted.</param>
    /// <returns>Buffer encrypted by the algorithm.</returns>
    public byte[] Encrypt(byte[] buffer)
    {
        using var encryptor = _aes!.CreateEncryptor();
        return encryptor.TransformFinalBlock(buffer, 0, buffer.Length);
    }

    /// <summary>
    /// Decrypt data buffer with the algorithm.
    /// </summary>
    /// <param name="buffer">Original buffer to be decrypted.</param>
    /// <returns>Buffer decrypted by the algorithm.</returns>
    public byte[] Decrypt(byte[] buffer)
    {
        using var decryptor = _aes!.CreateDecryptor();
        return decryptor.TransformFinalBlock(buffer, 0, buffer.Length);
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposing) return;
        if (_aes is null) return;
        _aes.Dispose();
        _aes = null;
    }
}