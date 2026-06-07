using System.Net;
using System.Text.RegularExpressions;
using Cirrus.Base.Services;
using Cirrus.LiveModels;
using Cirrus.Models.Business.Network;
using Cirrus.Network;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Cirrus.ViewModels;

public partial class NetworkProxyConfigDialogContentViewModel : ObservableObject
{
    public NetworkProxyConfigDialogContentViewModel()
    {
        AvailableProxyModes =
        [
            new("Dialogs/NetworkProxyConfigDialogContent/ProxyModes/Off")
                { Attachment = ProxyMode.NoProxy },

            new("Dialogs/NetworkProxyConfigDialogContent/ProxyModes/System")
                { Attachment = ProxyMode.SystemProxy },

            new("Dialogs/NetworkProxyConfigDialogContent/ProxyModes/Custom")
                { Attachment = ProxyMode.CustomProxy }
        ];
        if (ServicesProvider.GetService<UserPreferenceService>() is { } service)
        {
            PreferredProxyMode = AvailableProxyModes.Find(m => m.Attachment == service.Network.ProxyMode) ??
                                 AvailableProxyModes[0];
            CustomProxyHost = service.Network.CustomProxyHost ?? string.Empty;
            CustomProxyPort = service.Network.CustomProxyPort?.ToString() ?? string.Empty;
            return;
        }

        PreferredProxyMode = AvailableProxyModes[0];
        CustomProxyHost = string.Empty;
        CustomProxyPort = string.Empty;
    }

    public List<LocalizedText<ProxyMode>> AvailableProxyModes { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCustomProxyMode))]
    public partial LocalizedText<ProxyMode> PreferredProxyMode { get; set; }

    [ObservableProperty] public partial string CustomProxyHost { get; set; }

    [ObservableProperty] public partial string CustomProxyPort { get; set; }

    public bool IsCustomProxyMode => PreferredProxyMode.Attachment == ProxyMode.CustomProxy;

    partial void OnPreferredProxyModeChanged(LocalizedText<ProxyMode> value)
    {
        CustomProxyHost = string.Empty;
        CustomProxyPort = string.Empty;
    }

    public void UpdateSettings()
    {
        if (ServicesProvider.GetService<UserPreferenceService>() is not { } service) return;
        int portNumber = 80;
        if (PreferredProxyMode.Attachment == ProxyMode.CustomProxy)
        {
            if (!HostnameRegEx().IsMatch(CustomProxyHost) &&
                !Ipv4RegEx().IsMatch(CustomProxyHost) &&
                !Ipv6RegEx().IsMatch(CustomProxyHost)) return;
            if (!int.TryParse(CustomProxyPort, out portNumber) ||
                portNumber < 0 || portNumber > 65535) return;
            service.Network.CustomProxyHost = CustomProxyHost;
            service.Network.CustomProxyPort = portNumber;
        }

        service.Network.ProxyMode = PreferredProxyMode.Attachment;
        Client.Proxy = PreferredProxyMode.Attachment switch
        {
            ProxyMode.SystemProxy => HttpClient.DefaultProxy,
            ProxyMode.CustomProxy => new WebProxy(CustomProxyHost, portNumber),
            _ => null
        };
    }

    [GeneratedRegex(@"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\\-]*[a-zA-Z0-9])\\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\\-]*[A-Za-z0-9])$")]
    private static partial Regex HostnameRegEx();

    [GeneratedRegex(@"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)(\.(?!$)|$)){4}$")]
    private static partial Regex Ipv4RegEx();

    [GeneratedRegex(@"(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))")]
    private static partial Regex Ipv6RegEx();
}