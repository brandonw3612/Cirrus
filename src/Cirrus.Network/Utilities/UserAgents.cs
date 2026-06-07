namespace Cirrus.Network.Utilities;

/// <summary>
/// User agents used to mock different devices when sending requests.
/// </summary>
internal class UserAgents
{
    /// <summary>
    /// Current singleton instance of User Agents.
    /// </summary>
    public static UserAgents Current => field ??= new();

    private UserAgents()
    {
        // Hidden constructor method.
    }
    
    // Mobile User Agents
    private const string AndroidEdge = "(Linux; Android 13; RMX2202) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Mobile Safari/537.36 EdgA/113.0.1774.38";
    private const string AndroidWeChat = "(Linux; Android 13; RMX2202 Build/TP1A.220905.001; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/107.0.5304.141 Mobile Safari/537.36 XWEB/5075 MMWEBSDK/20230405 MMWEBID/5463 MicroMessenger/8.0.35.2360(0x2800235B) WeChat/arm64 Weixin NetType/WIFI Language/zh_CN ABI/arm64";
    private const string HarmonyOsBrowser = "(Linux; Android 12; HarmonyOS; NOH-AN01; HMSCore 6.10.4.302; GMSCore 22.15.14) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.88 HuaweiBrowser/13.0.6.302 Mobile Safari/537.36";
    private const string HarmonyOsWeChat = "(Linux; Android 12; NOH-AN01 Build/HUAWEINOH-AN01; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/107.0.5304.141 Mobile Safari/537.36 XWEB/5075 MMWEBSDK/20230405 MMWEBID/2312 MicroMessenger/8.0.35.2360(0x2800235B) WeChat/arm64 Weixin NetType/5G Language/zh_CN ABI/arm64";
    private const string IosEdge = "Mozilla/5.0 (iPhone; CPU iPhone OS 16_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) EdgiOS/113.0.1774.42 Version/16.0 Mobile/15E148 Safari/604.1";
    private const string IosFirefox = "(iPhone; CPU iPhone OS 16_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) FxiOS/113.2  Mobile/15E148 Safari/605.1.15";
    private const string IosSafari = "Mozilla/5.0 (iPhone; CPU iPhone OS 16_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.5 Mobile/15E148 Safari/604.1";
    private const string IosWeChat = "(iPhone; CPU iPhone OS 16_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148 MicroMessenger/8.0.37(0x1800252a) NetType/WIFI Language/zh_CN";
    private const string IpadOsEdge = "(iPad; CPU OS 16_4 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) EdgiOS/113.0.1774.42 Version/16.0 Mobile/15E148 Safari/604.1";
    private const string IpadOsFirefox = "(iPad; CPU iPhone OS 16_4_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) FxiOS/113.2  Mobile/15E148 Safari/605.1.15";
    private const string IpadOsSafari = "(iPad; CPU OS 16_4_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.4 Mobile/15E148 Safari/604.1";
    private const string IpadOsWeChat = "(iPad; CPU OS 16_4_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148 MicroMessenger/8.0.37(0x18002529) NetType/WIFI Language/en";
    
    // Desktop User Agents
    public const string LinuxChrome = "(X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36";
    private const string LinuxFirefox = "(X11; Ubuntu; Linux x86_64; rv:109.0) Gecko/20100101 Firefox/111.0";
    private const string MacOsEdge = "(Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36 Edg/113.0.1774.50";
    private const string MacOsFirefox = "(Macintosh; Intel Mac OS X 10.15; rv:109.0) Gecko/20100101 Firefox/113.0";
    private const string MacOsSafari = "(Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.5 Safari/605.1.15";
    private const string MacOsWeChat = "(Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36 NetType/WIFI MicroMessenger/6.8.0(0x16080000) MacWechat/3.8(0x13080010) XWEB/30515 Flue";
    private const string WindowsEdge = "(Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36 Edg/113.0.1774.57";
    private const string WindowsFirefox = "(Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/113.0";
    private const string WindowsWeChat = "(Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36 NetType/WIFI MicroMessenger/7.0.20.1781(0x6700143B) WindowsWechat(0x6309001c) XWEB/6609";

    /// <summary>
    /// User Agent platform.
    /// </summary>
    public enum Platform
    {
        /// <summary>
        /// Desktop User Agent. Including Linux, macOS and Windows.
        /// </summary>
        Desktop,
        /// <summary>
        /// Mobile User Agent. Including Android, HarmonyOS, iOS and iPadOS.
        /// </summary>
        Mobile,
        /// <summary>
        /// Any platform, containing <see cref="Desktop"/> and <see cref="Mobile"/>.
        /// </summary>
        Unspecified
    }

    // Build dictionary for specific platforms.
    private readonly Dictionary<Platform, string[]> _platformSpecific = new()
    {
        [Platform.Desktop] = new[]
        {
            LinuxChrome, LinuxFirefox,
            MacOsEdge, MacOsFirefox, MacOsSafari, MacOsWeChat,
            WindowsEdge, WindowsFirefox, WindowsWeChat
        },
        [Platform.Mobile] = new[]
        {
            AndroidEdge, AndroidWeChat,
            HarmonyOsBrowser, HarmonyOsWeChat,
            IosEdge, IosFirefox, IosSafari, IosWeChat,
            IpadOsEdge, IpadOsFirefox, IpadOsSafari, IpadOsWeChat
        }
    };

    /// <summary>
    /// Gets a random User Agent under specific platform.
    /// </summary>
    /// <param name="platform">The Specified platform.</param>
    public string this[Platform platform]
    {
        get
        {
            // Platform is unspecified. We randomly pick a User Agent in all platforms.
            if (platform is Platform.Unspecified)
            {
                int totalCount = _platformSpecific.Sum(p => p.Value.Length);
                int index = new Random().Next(totalCount);
                foreach (var p in _platformSpecific)
                {
                    if (index < p.Value.Length)
                    {
                        return p.Value[index];
                    }
                    index -= p.Value.Length;
                }
                return string.Empty;
            }
            // Platform is Desktop or Mobile.
            var platformUserAgents = _platformSpecific[platform];
            return platformUserAgents[new Random().Next(platformUserAgents.Length)];
        }
    }
}