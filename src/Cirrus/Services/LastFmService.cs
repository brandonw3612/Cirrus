using System.Text.Json;
using Cirrus.Base.Services;
using Cirrus.Base.Services.Abstract;
using Cirrus.Models.Network.Track;
using LiteFM;
using LiteFM.Abstractions;
using LiteFM.Abstractions.ApiContracts;
using LiteFM.Api;

namespace Cirrus.Services;

public class LastFmService : IScrobbleService
{
    private const string AuthBaseUrl = "https://www.last.fm/api/auth/";
    private const string ApiKeyConfigKey = "LastFm:ApiKey";
    private const string ApiSecretConfigKey = "LastFm:ApiSecret";

    private readonly UserPreferenceService _userPreferenceService;
    private readonly LastFMOptions _options;
    private readonly LastFMClient _client;

    public LastFMSession? Session { get; private set; }

    public LastFMUser? CurrentUser { get; private set; }

    public bool IsAuthenticated => Session is { HasLogined: true };

    public LastFmService(IConfigurationService configurationService, UserPreferenceService userPreferenceService)
    {
        _userPreferenceService = userPreferenceService;

        var configuration = configurationService.GetConfiguration();
        _options = new LastFMOptions
        {
            ApiKey = configuration[ApiKeyConfigKey] ?? string.Empty,
            ApiSecret = configuration[ApiSecretConfigKey] ?? string.Empty
        };
        _client = new LastFMClient(_options);

        RestoreSession();
    }

    public Uri GetAuthenticationUri(string? callbackUrl = null)
    {
        var url = $"{AuthBaseUrl}?api_key={Uri.EscapeDataString(_options.ApiKey)}";
        if (!string.IsNullOrWhiteSpace(callbackUrl))
        {
            url += $"&cb={Uri.EscapeDataString(callbackUrl)}";
        }
        return new Uri(url);
    }

    public async Task<bool> AuthenticateAsync(string token, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(token)) return false;

        var request = new GetSessionRequest
        {
            Token = token
        };
        var result = await _client.RequestAsync(LastFMApi.GetSessionApi, request);
        if (!result.IsSuccess || result.Response?.Session is not { HasLogined: true } session)
        {
            return false;
        }

        Session = session;
        _userPreferenceService.Account.LastFmSessionJson =
            JsonSerializer.Serialize(session, LastFMJsonDefaultContext.Default.LastFMSession);
        return true;
    }

    public async Task<LastFMUser?> GetUserInfoAsync(string? username = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var targetUser = string.IsNullOrWhiteSpace(username) ? Session?.Name : username;
        if (string.IsNullOrWhiteSpace(targetUser)) return null;

        var request = new GetUserInfoRequest
        {
            User = targetUser
        };
        var result = Session is { HasLogined: true }
            ? await _client.RequestAsync(LastFMApi.GetUserInfoApi, request, Session)
            : await _client.RequestAsync(LastFMApi.GetUserInfoApi, request);

        if (!result.IsSuccess || result.Response?.User is not { } user)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(username) ||
            string.Equals(username, Session?.Name, StringComparison.OrdinalIgnoreCase))
        {
            CurrentUser = user;
        }

        return user;
    }

    public void LogOut()
    {
        Session = null;
        CurrentUser = null;
        _userPreferenceService.Account.LastFmSessionJson = null;
    }

    public async Task RefreshStatusAsync(TrackBrief track, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (Session is not { HasLogined: true }) return;

        var request = new UpdateNowPlayingRequest
        {
            Track = track.Title,
            Artist = track.Artist,
            Album = track.Album
        };
        await _client.RequestAsync(LastFMApi.UpdateNowPlayingApi, request, Session);
    }

    public async Task ScrobbleAsync(TrackBrief track, DateTimeOffset startedAt, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (Session is not { HasLogined: true }) return;

        var request = new ScrobbleRequest
        {
            Track = track.Title,
            Artist = track.Artist,
            Album = track.Album,
            TimeStamp = (uint)Math.Max(0, startedAt.ToUnixTimeSeconds())
        };
        await _client.RequestAsync(LastFMApi.ScrobbleApi, request, Session);
    }

    private void RestoreSession()
    {
        var sessionJson = _userPreferenceService.Account.LastFmSessionJson;
        if (string.IsNullOrWhiteSpace(sessionJson)) return;

        try
        {
            var session = JsonSerializer.Deserialize(sessionJson, LastFMJsonDefaultContext.Default.LastFMSession);
            if (session is { HasLogined: true })
            {
                Session = session;
            }
        }
        catch (JsonException)
        {
            _userPreferenceService.Account.LastFmSessionJson = null;
        }
    }
}
