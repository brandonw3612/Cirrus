using System.Text;
using Cirrus.Network.Utilities;

namespace Cirrus.Network.EncryptionHandlers;

/// <summary>
/// Constants used by encryption handlers to encrypt messages.
/// </summary>
internal static class Constants
{
    private const string AesInitializationVectorContent = "0102030405060708";
    public static byte[] AesInitializationVector => field ??= Encoding.UTF8.GetBytes(AesInitializationVectorContent);

    private const string InnerAesSecretKeyContent = "0CoJUm6Qyw8W8jud";
    public static byte[] InnerAesSecretKey => field ??= Encoding.UTF8.GetBytes(InnerAesSecretKeyContent);

    private const string LinuxApiKeyContent = "rFgB&h#%2?^eDg:Q";
    public static byte[] LinuxApiSecretKey => field ??= Encoding.UTF8.GetBytes(LinuxApiKeyContent);

    private const string RsaPublicKeyExponentBase64 = "AQAB";
    private const string RsaPublicKeyModulusBase64 = "4LUJ9iWd+GQtvDVmKQFHffImd+wVK1/2is5hW7e3JRUrOrF6h2rqilqnbS5BdinsTuNB9WE1/M9pUoAQTgMS7L2pJVfJOHARSvbJ0FxPfww2hbeka+4lWTJXXM4QtCTYE8/kh10+ggR7l93vUnQdVGuOKJ3Gk1s+zgRi2woiuOc=";
    public static Rsa Rsa => field ??= new(Convert.FromBase64String(RsaPublicKeyExponentBase64), Convert.FromBase64String(RsaPublicKeyModulusBase64));
    
    public const string Base62 = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    private const string EapiSecretKeyContent = "e82ckenh8dichen8";
    public static byte[] EapiSecretKey => field ??= Encoding.UTF8.GetBytes(EapiSecretKeyContent);
}