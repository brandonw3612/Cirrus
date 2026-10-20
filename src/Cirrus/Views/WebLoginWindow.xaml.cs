using Cirrus.Base.Services;
using Cirrus.Base.Services.Abstract;
using Cirrus.Primitives;
using Cirrus.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.Web.WebView2.Core;

namespace Cirrus.Views;

public abstract class WebLoginWindowBase : AsyncChildWindow<bool>;

public sealed partial class WebLoginWindow
{
    private const string CallbackUrl = "http://localhost/lastfm-callback";

    private readonly LastFmService? _lastFmService;
    private bool _isAuthenticating;

    public WebLoginWindow()
    {
        _lastFmService = ServicesProvider.Current.GetServices<IScrobbleService>()
            .OfType<LastFmService>()
            .SingleOrDefault();
        InitializeComponent();
        Closed += OnWindowClosed;
    }

    protected override async Task OnInitializeAsync()
    {
        if (_lastFmService is null)
        {
            ReturnWith(false);
            return;
        }

        var env = await CoreWebView2Environment.CreateAsync();
        await WebView.EnsureCoreWebView2Async(env);
        WebView.CoreWebView2.NavigationStarting += OnNavigationStarting;
        WebView.CoreWebView2.NavigationCompleted += OnNavigationCompleted;
        WebView.Source = _lastFmService.GetAuthenticationUri(CallbackUrl);
    }

    private async void OnNavigationStarting(CoreWebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
        if (!args.Uri.StartsWith(CallbackUrl, StringComparison.OrdinalIgnoreCase))
        {
            LoadingProgressBar.Visibility = Visibility.Visible;
            return;
        }

        args.Cancel = true;
        if (_isAuthenticating || _lastFmService is null) return;
        _isAuthenticating = true;
        LoadingProgressBar.Visibility = Visibility.Visible;

        var token = ExtractTokenFromUri(args.Uri);
        if (string.IsNullOrWhiteSpace(token))
        {
            ReturnWith(false);
            return;
        }

        var succeeded = await _lastFmService.AuthenticateAsync(token);
        if (succeeded)
        {
            await _lastFmService.GetUserInfoAsync();
        }
        ReturnWith(succeeded);
    }

    private void OnNavigationCompleted(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        if (!_isAuthenticating)
        {
            LoadingProgressBar.Visibility = Visibility.Collapsed;
        }
    }

    private void OnWindowClosed(object sender, WindowEventArgs args)
    {
        Closed -= OnWindowClosed;
        if (WebView.CoreWebView2 is { } coreWebView)
        {
            coreWebView.NavigationStarting -= OnNavigationStarting;
            coreWebView.NavigationCompleted -= OnNavigationCompleted;
        }
    }

    private static string? ExtractTokenFromUri(string uriString)
    {
        if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri)) return null;
        var query = uri.Query.TrimStart('?');
        if (string.IsNullOrWhiteSpace(query)) return null;

        foreach (var part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = part.Split('=', 2);
            if (kv.Length == 2 && string.Equals(kv[0], "token", StringComparison.OrdinalIgnoreCase))
            {
                return Uri.UnescapeDataString(kv[1]);
            }
        }
        return null;
    }
}
