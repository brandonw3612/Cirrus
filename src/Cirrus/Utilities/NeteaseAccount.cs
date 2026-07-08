using System.Text.RegularExpressions;
using Cirrus.Base.Services;
using Cirrus.Models.Network.Account;
using Cirrus.Models.Network.Response.Authentication;
using Cirrus.Models.Network.Response.User;
using Cirrus.Models.Network.User;
using Cirrus.Network;
using Cirrus.Utilities.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Cirrus.Utilities;

public partial class NeteaseAccount : ObservableObject, IDisposable
{
    [ObservableProperty] public partial bool IsLoggedIn { get; private set; }    
    [ObservableProperty] public partial UserProfile? UserProfile { get; private set; }
    [ObservableProperty] public partial UserAccount? UserAccount { get; private set; }
    [ObservableProperty] public partial UserMembershipStatusApiResponse? MembershipStatus { get; private set; }
    [ObservableProperty] public partial QuickAccessPlaylistCollection? QuickAccessPlaylists { get; private set; }
    
    [GeneratedRegex(@"/^[\w\-\.]+@([\w-]+\.)+[\w-]{2,}$/gm")]
    private static partial Regex EmailRegex();

    private readonly LocalizedExceptionService _exceptionService;
    
    private NeteaseAccount(bool isLoggedIn, UserProfile? userProfile, UserAccount? userAccount,
        UserMembershipStatusApiResponse? membershipStatus, QuickAccessPlaylistCollection? quickAccessPlaylists)
    {
        IsLoggedIn = isLoggedIn;
        UserProfile = userProfile;
        UserAccount = userAccount;
        MembershipStatus = membershipStatus;
        QuickAccessPlaylists = quickAccessPlaylists;
        _exceptionService = ServicesProvider.GetService<LocalizedExceptionService>()!;
    }

    public static async Task<NeteaseAccount> LoadAsync()
    {
        var loginStatus = await Client.Authentication.GetLoginStatusAsync();
        UserMembershipStatusApiResponse? membershipStatus = null;
        QuickAccessPlaylistCollection? quickAccessPlaylistCollection = null;
        if (loginStatus.IsLoggedIn)
        {
            membershipStatus = await Client.User.GetMembershipStatusAsync(loginStatus.Account!.UserId);
            quickAccessPlaylistCollection = new(loginStatus.Account.UserId);
            await quickAccessPlaylistCollection.LoadAsync();
        }
        NeteaseAccount account = new(loginStatus.IsLoggedIn, loginStatus.Profile, loginStatus.Account, membershipStatus,
            quickAccessPlaylistCollection);
        return account;
    }

    public async Task<AuthenticationResult> LoginAsync(string phoneNumber, string password, string countryCode)
    {
        phoneNumber = string.Join(string.Empty, phoneNumber.ToCharArray().Where(i => i is >= '0' and <= '9'));
        var extractedCountryCode = string.Join(string.Empty, countryCode.ToCharArray().Where(i => i is >= '0' and <= '9'));
        if (extractedCountryCode is { Length: 0 }) extractedCountryCode = null;
        var authResponse = await Client.Authentication.PhonePasswordLoginAsync
            (phoneNumber, password, extractedCountryCode);
        return await HandleResponseAsync(authResponse);
    }

    public async Task<AuthenticationResult> LoginAsync(string mailAddress, string password)
    {
        if (!EmailRegex().IsMatch(mailAddress))
        {
            var exception = _exceptionService.CreateLocalized("Exceptions/Authentication/InvalidEmailException");
            return AuthenticationResult.CreateFailure(exception);
        }
        var authResponse = await Client.Authentication.MailLoginAsync(mailAddress, password);
        return await HandleResponseAsync(authResponse);
    }

    public async Task LogOutAsync()
    {
        var response = await Client.Authentication.LogOutAsync();
        Client.UserCredentials.Clear();
        if (response.StatusCode is 200)
        {
            UserAccount = null;
            UserProfile = null;
            MembershipStatus = null;
            QuickAccessPlaylists = null;
            IsLoggedIn = false;
        }
    }

    public async Task ReloadAsync()
    {
        var loginStatus = await Client.Authentication.GetLoginStatusAsync();
        IsLoggedIn = loginStatus.IsLoggedIn;
        UserProfile = loginStatus.Profile;
        UserAccount = loginStatus.Account;
        if (loginStatus.IsLoggedIn)
        {
            MembershipStatus = await Client.User.GetMembershipStatusAsync(loginStatus.Account!.UserId);
            QuickAccessPlaylists?.Dispose();
            QuickAccessPlaylists = new(loginStatus.Account.UserId);
            await QuickAccessPlaylists.LoadAsync();
        }
        else
        {
            QuickAccessPlaylists?.Dispose();
            QuickAccessPlaylists = null;
        }
    }
    
    private async Task<AuthenticationResult> HandleResponseAsync(LoginApiResponse response)
    {
        if (response is
            {
                StatusCode: 200,
                Profile: { } profile,
                Account: { } account
            })
        {
            IsLoggedIn = true;
            UserProfile = profile;
            UserAccount = account;
            MembershipStatus = await Client.User.GetMembershipStatusAsync(account!.UserId);
            return AuthenticationResult.CreateSuccess();
        }
        IsLoggedIn = false;
        var exception = _exceptionService.CreateLocalized("Exceptions/Authentication/LoginFailedExceptionFormat",
            response.ResponseMessage);
        return AuthenticationResult.CreateFailure(exception);
    }

    public void Dispose()
    {
        QuickAccessPlaylists?.Dispose();
    }
}