using System.Net;
using Cirrus.Primitives;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Web.WebView2.Core;

namespace Cirrus.Views;

public abstract class WebLoginWindowBase : AsyncChildWindow<bool>;

public sealed partial class WebLoginWindow
{
    public WebLoginWindow()
    {
        InitializeComponent();
    }

    protected override async Task OnInitializeAsync()
    {
        var env = await CoreWebView2Environment.CreateAsync();
        await WebView.EnsureCoreWebView2Async(env);
        WebView.Source = new("https://music.163.com/login");  
    }

    [RelayCommand]
    private async Task VerifyAsync()
    {
        var cookies = await WebView.CoreWebView2.CookieManager.GetCookiesAsync("https://music.163.com");
        if (cookies.Count == 0) return;
        CookieCollection cookieCollection = new();
        foreach (var cookie in cookies)
        {
            cookieCollection.Add(new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
        }
        Network.Client.Cookies = cookieCollection;
        var status = await Network.Client.Authentication.GetLoginStatusAsync();
        if (status.IsLoggedIn)
        {
            ReturnWith(true);
            return;
        }
        Network.Client.Cookies = new();
    } 
}
