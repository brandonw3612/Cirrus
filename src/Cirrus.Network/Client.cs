using System.Net;
using System.Text.Json.Nodes;
using Cirrus.Base.Services;
using Cirrus.Generated.Attributes;
using Cirrus.Models.Business.Network;
using Cirrus.Network.Api;
using Cirrus.Network.Diagnostics;
using Constants = Cirrus.Network.Api.Constants;

namespace Cirrus.Network;

/// <summary>
/// Music API Client. Entry point for all available APIs.
/// </summary>
/// <remarks>
/// Method <see cref="Initialize"/> is required to be called before any operation.
/// </remarks>
/// <example>
/// 1. Initialize the client before any operation.
/// <code>await Networking.Client.Initialize();</code>
/// 2. Configure the client (if needed, available at any time after initialization).
/// <code>Networking.Client.IpFix = "________________";</code>
/// 3. Call the APIs.
/// <code>var response = await Client.User.GetDetailAsync(__________);</code>
/// </example>
public partial class Client
{
    private static Client? _current;
    /// <summary>
    /// Current singleton instance of the client.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Client has not been initialized. <see cref="Initialize"/> has not been called.
    /// </exception>
    internal static Client Current => _current ?? throw new InvalidOperationException("Client is not initialized.");

    private static readonly object InitializationLock = new();

    internal IDeserializationCoverageService? DeserializationCoverageService { get; private set; }
    private readonly UserPreferenceService? _userPreferenceService;
    
    #region Configurations

    private string? _ipFix;
    private IWebProxy? _proxy;
    private CookieCollection _cookies;
    private JsonObject _userCredentials;

    #endregion

    #region API Containers

    private Account? _account;
    private Advanced? _advanced;
    private Album? _album;
    private Artist? _artist;
    private Authentication? _authentication;
    private Playlist? _playlist;
    private Public? _public;
    private Search? _search;
    private Track? _track;
    private User? _user;

    #endregion
    
    private Client()
    {
        // Hidden constructor method.
        DeserializationCoverageService = ServicesProvider.GetService<IDeserializationCoverageService>();
        _userPreferenceService = ServicesProvider.GetService<UserPreferenceService>();
        _cookies = _userPreferenceService?.Account.UserCookies ?? new();
        _userCredentials = _userPreferenceService?.Account.UserCredentials ?? new();
        _ipFix = _userPreferenceService?.Network.IpFix;
        _proxy = _userPreferenceService?.Network.ProxyMode switch
        {
            ProxyMode.SystemProxy => HttpClient.DefaultProxy,
            ProxyMode.CustomProxy when _userPreferenceService.Network is
                { CustomProxyHost: { } host, CustomProxyPort: { } port } => new WebProxy(host, port),
            _ => null
        };
        Constants.Protocol = _userPreferenceService?.Network.IsHttpFallbackEnabled is true ? "http" : "https";
    }
    
    /// <summary>
    /// Initialize the client restoring user state.
    /// </summary>
    public static Task Initialize()
    {
        if (_current is not null) return Task.CompletedTask;
        lock (InitializationLock)
        {
            if (_current is not null) return Task.CompletedTask;
            _current = new();
            return Task.CompletedTask;
        }
    }

    #region Public static accessors for configurations

    /// <summary>
    /// IP address used in requests to resolve accessibility issues in certain network environments.
    /// </summary>
    public static string? IpFix
    {
        get => Current._ipFix;
        set
        {
            Current._ipFix = value;
            if (Current._userPreferenceService is { } userPreferenceService)
                userPreferenceService.Network.IpFix = value;
        }
    }

    /// <summary>
    /// Proxy that the client is using.
    /// </summary>
    public static IWebProxy? Proxy
    {
        get => Current._proxy;
        set => Current._proxy = value;
    }

    /// <summary>
    /// Cookies that the client puts in the headers on every request.
    /// </summary>
    public static CookieCollection Cookies
    {
        get => Current._cookies;
        set
        {
            Current._cookies = value;
            if (Current._userPreferenceService is { } userPreferenceService)
                userPreferenceService.Account.UserCookies = value;
        }
    }

    public static JsonObject UserCredentials
    {
        get => Current._userCredentials;
        set
        {
            Current._userCredentials = value;
            if (Current._userPreferenceService is { } userPreferenceService)
                userPreferenceService.Account.UserCredentials = value;
        }
    }

    #endregion

    #region Public static accessors for API Containers

    /// <summary>
    /// Account APIs provided by the singleton instance of the client.
    /// </summary>
    [MusicApiContainer("account")]
    public static Account Account => Current._account ??= new();

    /// <summary>
    /// Advanced APIs provided by the singleton instance of the client.
    /// </summary>
    [MusicApiContainer("advanced")]
    public static Advanced Advanced => Current._advanced ??= new();
    
    /// <summary>
    /// Album APIs provided by the singleton instance of the client.
    /// </summary>
    [MusicApiContainer("album")]
    public static Album Album => Current._album ??= new();

    /// <summary>
    /// Artist APIs provided by the singleton instance of the client.
    /// </summary>
    [MusicApiContainer("artist")]
    public static Artist Artist => Current._artist ??= new();

    /// <summary>
    /// Authentication APIs provided by the singleton instance of the client.
    /// </summary>
    [MusicApiContainer("authentication")]
    public static Authentication Authentication => Current._authentication ??= new();
    
    /// <summary>
    /// Playlist APIs provided by the singleton instance of the client.
    /// </summary>
    [MusicApiContainer("playlist")]
    public static Playlist Playlist => Current._playlist ??= new();

    /// <summary>
    /// Public APIs provided by the singleton instance of the client.
    /// </summary>
    [MusicApiContainer("public")]
    public static Public Public => Current._public ??= new();
    
    /// <summary>
    /// Search APIs provided by the singleton instance of the client.
    /// </summary>
    [MusicApiContainer("search")]
    public static Search Search => Current._search ??= new();

    /// <summary>
    /// Track APIs provided by the singleton instance of the client.
    /// </summary>
    [MusicApiContainer("track")]
    public static Track Track => Current._track ??= new();
    
    /// <summary>
    /// User APIs provided by the singleton instance of the client.
    /// </summary>
    [MusicApiContainer("user")]
    public static User User => Current._user ??= new();

    #endregion
}