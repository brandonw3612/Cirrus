using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;
using Cirrus.Models.Network.Response;
using Cirrus.Models.Serialization;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Exceptions;
using Cirrus.Network.Utilities;

namespace Cirrus.Network.Primitives;

/// <summary>
/// Url parser delegate. Used when Url of the request is dynamically generated with the parameter.
/// </summary>
/// <typeparam name="TParameter">Type of the parameter passed to the API.</typeparam>
internal delegate string UrlParser<in TParameter>(TParameter parameter);

/// <summary>
/// Body parser delegate. Used when body of the request is generated with the parameter.
/// </summary>
/// <typeparam name="TParameter">Type of the parameter passed to the API.</typeparam>
internal delegate Dictionary<string, object>? BodyParser<in TParameter>(TParameter parameter);

internal interface IMusicApi;

internal interface IFixedMusicApi : IMusicApi
{
    string Url { get; }
    Dictionary<string, object> Body { get; }
    Type ResponseType { get; }
}

/// <summary>
/// Parameterless music API definition.
/// </summary>
/// <typeparam name="TResponse">Response type of the API. Must be derived from <see cref="MusicApiResponse"/>.</typeparam>
internal class MusicApi<TResponse> : IFixedMusicApi where TResponse : MusicApiResponse
{
    private readonly string _route;
    private readonly HttpMethod _method;
    private readonly string _url;
    private readonly Dictionary<string, object> _body;
    private readonly ApiConfig _config;

    public string Url => _url;
    public Dictionary<string, object> Body => _body;
    public Type ResponseType => typeof(TResponse);
    
    /// <summary>
    /// Constructs a music API.
    /// </summary>
    /// <param name="route">Internal route of the API.</param>
    /// <param name="method">Request method used by the API.</param>
    /// <param name="config">API configuration.</param>
    /// <param name="url">Plain url of the request.</param>
    /// <param name="body">Default body of the request.</param>
    public MusicApi(string route, HttpMethod method, ApiConfig config, string url, Dictionary<string, object>? body = null)
    {
        _route = route;
        _method = method;
        _config = config;
        _url = url;
        _body = body ?? new();
    }

    /// <summary>
    /// Sends an API request and return the response.
    /// </summary>
    /// <param name="preventCache">Whether response should be prevented from being cached.</param>
    /// <returns>Response of the request.</returns>
    public Task<TResponse> RequestAsync(bool preventCache = false) => RequestAsync<TResponse>(preventCache);
    
    /// <summary>
    /// Sends an API request and return the response with custom type. 
    /// </summary>
    /// <typeparam name="TCustomResponse">Response type other than that declared in the definition of the API.</typeparam>
    /// <param name="preventCache">Whether response should be prevented from being cached.</param>
    /// <returns>Response of the request, with the provided type.</returns>
    public Task<TCustomResponse> RequestAsync<TCustomResponse>(bool preventCache = false) where TCustomResponse : MusicApiResponse
    {
        var requestUrl = _url;
        Dictionary<string, object> body = new();
        foreach (var (key, obj) in _body)
        {
            body[key] = obj;
        }
        if (preventCache)
        {
            body["timestamp"] = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
        return RequestInvoker.RequestAsync<TCustomResponse>(requestUrl, body, _route, _method, _config);
    }
}

internal interface IDynamicMusicApi : IMusicApi
{
    Type ParameterType { get; }
    Type ResponseType { get; }
    UrlParser<object> UrlParser { get; }
    BodyParser<object>? BodyParser { get; }
}

/// <summary>
/// Parameterized music API definition.
/// </summary>
/// <typeparam name="TParameter">Parameter type of the API.</typeparam>
/// <typeparam name="TResponse">Response type of the API. Must be derived from <see cref="MusicApiResponse"/>.</typeparam>
internal class MusicApi<TParameter, TResponse> : IDynamicMusicApi where TResponse : MusicApiResponse
{
    private readonly string _route;
    private readonly HttpMethod _method;
    private readonly UrlParser<TParameter> _urlParser;
    private readonly BodyParser<TParameter>? _bodyParser;
    private readonly ApiConfig _config;

    public UrlParser<TParameter> UrlParser => _urlParser;
    public BodyParser<TParameter>? BodyParser => _bodyParser;
    
    Type IDynamicMusicApi.ParameterType => typeof(TParameter);
    Type IDynamicMusicApi.ResponseType => typeof(TResponse);
    
    UrlParser<object> IDynamicMusicApi.UrlParser => p => p is TParameter pp ? _urlParser(pp) :
        throw new ArgumentException($"Parameter must be of type {typeof(TParameter).FullName}", nameof(p));

    BodyParser<object>? IDynamicMusicApi.BodyParser => _bodyParser is null
        ? null
        : p =>
        {
            if (p is not TParameter pp)
            {
                throw new ArgumentException($"Parameter must be of type {typeof(TParameter).FullName}", nameof(p));
            }
            return _bodyParser(pp);
        };
    
    /// <summary>
    /// Constructs a music API with plain request Url.
    /// </summary>
    /// <param name="route">Internal route of the API.</param>
    /// <param name="method">Request method used by the API.</param>
    /// <param name="config">API configuration.</param>
    /// <param name="plainUrl">Plain url of the request.</param>
    /// <param name="bodyParser">Body parser of the request.</param>
    public MusicApi(string route, HttpMethod method, ApiConfig config, string plainUrl, BodyParser<TParameter>? bodyParser = null) :
        this(route, method, config, _ => plainUrl, bodyParser)
    {
        // Ignored body.
    }
    
    /// <summary>
    /// Constructs a music API with Url parser.
    /// </summary>
    /// <param name="route">Internal route of the API.</param>
    /// <param name="method">Request method used by the API.</param>
    /// <param name="config">API configuration.</param>
    /// <param name="urlParser">Url parser of the request.</param>
    /// <param name="bodyParser">Body parser of the request.</param>
    public MusicApi(string route, HttpMethod method, ApiConfig config, UrlParser<TParameter> urlParser, BodyParser<TParameter>? bodyParser = null)
    {
        _route = route;
        _method = method;
        _config = config;
        _urlParser = urlParser;
        _bodyParser = bodyParser;
    }

    /// <summary>
    /// Sends an API request and return the response.
    /// </summary>
    /// <param name="parameter">Parameter of the API.</param>
    /// <param name="preventCache">Whether response should be prevented from being cached.</param>
    /// <returns>Response of the request.</returns>
    public Task<TResponse> RequestAsync(TParameter parameter, bool preventCache = false) => RequestAsync<TResponse>(parameter, preventCache);
    
    /// <summary>
    /// Sends an API request and return the response with custom type. 
    /// </summary>
    /// <typeparam name="TCustomResponse">Response type other than that declared in the definition of the API.</typeparam>
    /// <param name="parameter">Parameter of the API.</param>
    /// <param name="preventCache">Whether response should be prevented from being cached.</param>
    /// <returns>Response of the request, with the provided type.</returns>
    public Task<TCustomResponse> RequestAsync<TCustomResponse>(TParameter parameter, bool preventCache = false) where TCustomResponse : MusicApiResponse
    {
        var requestUrl = _urlParser.Invoke(parameter);
        var originalRequestBody = _bodyParser?.Invoke(parameter) ?? new();
        if (preventCache)
        {
            originalRequestBody["timestamp"] = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
        return RequestInvoker.RequestAsync<TCustomResponse>(requestUrl, originalRequestBody, _route, _method, _config);
    }
}

/// <summary>
/// Utility for sending requests with different API configurations.
/// </summary>
internal static class RequestInvoker
{
    /// <summary>
    /// Sends a request with provided configuration and returns the response.
    /// </summary>
    /// <param name="url">Url of the request.</param>
    /// <param name="body">Body of the request.</param>
    /// <param name="apiRoute">Internal API route.</param>
    /// <param name="method">Method of the request.</param>
    /// <param name="apiConfig">API configuration.</param>
    /// <typeparam name="TResponse">Response type of the request. Must be derived from <see cref="MusicApiResponse"/>.</typeparam>
    /// <returns>Response of the request.</returns>
    /// <exception cref="Exception">Thrown when an error occurred during deserialization of the response data.</exception>
    internal static async Task<TResponse> RequestAsync<TResponse>(string url, Dictionary<string, object> body,
        string apiRoute, HttpMethod method, ApiConfig apiConfig) where TResponse : MusicApiResponse
    {
        Dictionary<string, string> requestHeaders = new()
        {
            ["User-Agent"] = UserAgents.Current[apiConfig.UserAgentPlatform]
        };
        if (method == HttpMethod.Post)
        {
            requestHeaders["Content-Type"] = "application/x-www-form-urlencoded";
        }
        if (url.Contains("music.163.com"))
        {
            requestHeaders["Referer"] = "https://music.163.com";
        }
        if (Client.IpFix is { } ipFix)
        {
            requestHeaders["X-Real-IP"] = ipFix;
            requestHeaders["X-Forwarded-For"] = ipFix;
        }
        // static string RandomBytesString(int length)
        // {
        //     var randomBytes = new byte[length];
        //     new Random().NextBytes(randomBytes);
        //     return Convert.ToHexString(randomBytes).ToLower();
        // }
        CookieCollection requestCookies = new()
        {
            Client.Cookies,
            apiConfig.CookiesFix,
            new Cookie("__remember_me", "true"),
            // Seems we cannot log in with these cookies.
            // new Cookie("NMTID", RandomBytesString(16)),
            // new Cookie("_ntes_nuid", RandomBytesString(16))
        };
        if (Client.UserCredentials?["Cookies"] is JsonObject cookiesCred)
        {
            foreach (var c in cookiesCred)
            {
                requestCookies.Add(new Cookie(c.Key, c.Value!.ToString()));
            }
        }
        if (requestCookies.All(static c => c.Name is not ("MUSIC_U" or "MUSIC_A")))
        {
            requestCookies.Add(new Cookie("MUSIC_A", Constants.AnonymousToken));
        }
        requestHeaders["Cookie"] = string.Join("; ", requestCookies
            .Select(static c => $"{HttpUtility.HtmlEncode(c.Name)}={HttpUtility.HtmlEncode(c.Value)}"));
        (url, requestHeaders, var requestBody) = apiConfig.EncryptionHandler.HandleRequest(
            url, method, requestHeaders, body, requestCookies, apiConfig.Url);
        using HttpClient client = new(new HttpClientHandler
        {
            UseProxy = Client.Proxy is not null,
            Proxy = Client.Proxy,
            UseCookies = false
        });
        client.DefaultRequestHeaders.CacheControl = new()
        {
            NoCache = true,
            NoStore = true
        };
        using HttpRequestMessage requestMessage = new(method, url);
        requestMessage.Content = new FormUrlEncodedContent(requestBody);
        foreach (var header in requestHeaders)
        {
            if (Constants.ContentHeaderNames.Contains(header.Key.Replace("-", string.Empty).ToLower()))
            {
                // Content header.
                if (requestMessage.Content.Headers.Contains(header.Key))
                {
                    requestMessage.Content.Headers.Remove(header.Key);
                }
                requestMessage.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            else
            {
                // Request header.
                if (requestMessage.Headers.Contains(header.Key))
                {
                    requestMessage.Headers.Remove(header.Key);
                }
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
        using var response = await client.SendAsync(requestMessage);
        if (response.Headers.Contains("Set-Cookie"))
        {
            var cookieStrings = response.Headers.GetValues("Set-Cookie");
            var parsedCookieCollection = ParseSetCookie(cookieStrings);
            var cookies = new CookieCollection
            {
                Client.Cookies,
                parsedCookieCollection
            };
            Client.Cookies = [..cookies.Where(static c => !c.Expired)];
        }
        var responseBody = await response.Content.ReadAsByteArrayAsync();
        if (apiConfig.EncryptionHandler is EapiHandler)
        {
            responseBody = EapiHandler.Decrypt(responseBody);
        }
        var jsonContent = Encoding.UTF8.GetString(responseBody);
        TResponse? responseObject;
        try
        {
            responseObject = JsonSerializer.Deserialize(
                jsonContent, typeof(TResponse), ModelsSerializationContext.Default
            ) as TResponse;
        }
        catch (JsonException e)
        {
            throw new DeserializationFailedException(e)
            {
                ApiRoute = apiRoute,
                PropertyPath = e.Path ?? "Unknown",
                ThrownTime = DateTimeOffset.Now
            };
        }
        if (responseObject is null)
        {
            throw new DeserializationFailedException(null)
            {
                ApiRoute = apiRoute,
                PropertyPath = "N/A",
                ThrownTime = DateTimeOffset.Now
            };
        }
        Client.Current.DeserializationCoverageService?.ProcessEntity(apiRoute, typeof(TResponse), jsonContent);
        if (Constants.SuccessStatusCodes.Contains(responseObject.StatusCode))
            return responseObject;
        throw new UnsuccessfulResponseException
        {
            ApiRoute = apiRoute,
            StatusCode = responseObject.StatusCode,
            ThrownTime = DateTimeOffset.Now
        };
    }

    /// <summary>
    /// Parse cookies to set when response containing a "Set-Cookie" field.
    /// </summary>
    /// <param name="cookieStrings">Cookie strings to set.</param>
    /// <returns>Parsed cookie collection.</returns>
    private static CookieCollection ParseSetCookie(IEnumerable<string> cookieStrings)
    {
        CookieCollection parsedCookies = new();
        foreach (var cookieString in cookieStrings)
        {
            var segments = cookieString.Split(';');
            var cookieContent = segments[0].Split('=');
            Cookie cookie = new(cookieContent[0].Trim(), cookieContent[1].Trim());
            for (var i = 1; i < segments.Length; i++)
            {
                var cookieProperty = segments[i].Split('=');
                var propertyName = cookieProperty[0].Trim();
                if (cookieProperty is { Length: 1 })
                {
                    switch (propertyName.ToLower())
                    {
                        case "httponly":
                            {
                                cookie.HttpOnly = true;
                                break;
                            }
                        case "discard":
                            {
                                cookie.Discard = true;
                                break;
                            }
                        case "secure":
                            {
                                cookie.Secure = true;
                                break;
                            }
                        case "expired":
                            {
                                cookie.Expired = true;
                                break;
                            }
                    }
                }
                else
                {
                    var propertyValue = cookieProperty[1].Trim();
                    switch (propertyName.ToLower())
                    {
                        case "max-age":
                            {
                                cookie.Expires = DateTime.Now.AddSeconds(long.Parse(propertyValue));
                                break;
                            }
                        case "expires":
                            {
                                cookie.Expires = DateTime.Parse(propertyValue);
                                break;
                            }
                        case "path":
                            {
                                cookie.Path = propertyValue;
                                break;
                            }
                        case "domain":
                            {
                                cookie.Domain = propertyValue;
                                break;
                            }
                    }
                }
            }
            parsedCookies.Add(cookie);
        }
        return parsedCookies;
    }
}