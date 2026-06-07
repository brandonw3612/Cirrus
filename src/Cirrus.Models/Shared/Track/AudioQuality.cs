namespace Cirrus.Models.Shared.Track;

/// <summary>
/// Audio quality parameter in API track/audio.
/// </summary>
public enum AudioQuality
{
    /// <summary>
    /// Standard audio quality, typically 128 kbps.
    /// </summary>
    Standard = 0,
    /// <summary>
    /// Higher audio quality, 320 kbps at highest.
    /// </summary>
    Higher = 1,
    /// <summary>
    /// Lossless audio quality, 48 kHz / 16 bit at highest.
    /// </summary>
    Lossless = 2,
    /// <summary>
    /// Hi-Res audio quality, 192 kHz / 24 bit at highest.
    /// </summary>
    HiRes = 3,
    /// <summary>
    /// Spatial audio quality, typically 96 kHz / 24 bit.
    /// </summary>
    Spatial = 4,
    /// <summary>
    /// Surround audio quality, 5.1 Channel at highest.
    /// </summary>
    Surround = 5,
    /// <summary>
    /// Master audio quality, typically 192 kHz / 24 bit.
    /// </summary>
    Master = 6
}