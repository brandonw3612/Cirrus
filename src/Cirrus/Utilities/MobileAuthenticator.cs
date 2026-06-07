using Cirrus.Network;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;

namespace Cirrus.Utilities;

public partial class MobileAuthenticator : ObservableObject
{
    private readonly DispatcherQueue _dispatcherQueue;

    [ObservableProperty]
    public partial string CurrentStatus { get; set; } =
        "Dialogs/LoginDialogContent/MobileAuth/FetchingKeyMessage".GetLocalized() ?? "{Invalid Resource}";

    private string? _qrCodeKey;
    public string QrCodeKey => "https://music.163.com" +
                               (_qrCodeKey is null ? string.Empty : $"/login?codekey={_qrCodeKey}");
    private readonly SemaphoreSlim _qrCodeKeySemaphore = new(1);

    private readonly Timer _qrStatusQueryTimer;
    private readonly Action _authenticatedAction;

    public MobileAuthenticator(DispatcherQueue dispatcherQueue, Action authenticatedAction)
    {
        _authenticatedAction = authenticatedAction;
        _dispatcherQueue = dispatcherQueue;
        _qrStatusQueryTimer = new(OnQueryTimerElapsed, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public void Activate()
    {
        _qrStatusQueryTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }
    
    public void Deactivate()
    {
        _qrStatusQueryTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }
    
    private async void OnQueryTimerElapsed(object? _)
    {
        if (_qrCodeKey is null)
        {
            await _dispatcherQueue.EnqueueAsync(FetchQrCodeAsync);
            return;
        }
        await _dispatcherQueue.EnqueueAsync(async () => await FetchQrCodeStatusAsync(_qrCodeKey));
    }

    private async Task FetchQrCodeAsync()
    {
        await _qrCodeKeySemaphore.WaitAsync();
        try
        {
            var response = await Client.Authentication.GetLoginQrCodeKeyAsync();
            if (response is { StatusCode: 200 })
            {
                _qrCodeKey = response.Key;
            }
            else
            {
                _qrCodeKey = null;
                CurrentStatus = "Dialogs/LoginDialogContent/MobileAuth/KeyFetchErrorMessage".GetLocalized() ??
                                "{Invalid Resource}";
            }
            OnPropertyChanged(nameof(QrCodeKey));
        }
        finally
        {
            _qrCodeKeySemaphore.Release();
        }
    }
    
    private async Task FetchQrCodeStatusAsync(string codeKey)
    {
        var response = await Client.Authentication.GetQrLoginStatusAsync(codeKey);
        switch (response.StatusCode)
        {
            case 800:
            {
                CurrentStatus = "Dialogs/LoginDialogContent/MobileAuth/QrExpiredMessage".GetLocalized() ??
                                "{Invalid Resource}";
                await _qrCodeKeySemaphore.WaitAsync();
                try
                {
                    _qrCodeKey = null;
                    OnPropertyChanged(nameof(QrCodeKey));
                }
                finally
                {
                    _qrCodeKeySemaphore.Release();
                }
                break;
            }
            case 801:
            {
                CurrentStatus = "Dialogs/LoginDialogContent/MobileAuth/AwaitingScanMessage".GetLocalized() ??
                                "{Invalid Resource}";
                break;
            }
            case 802:
            {
                CurrentStatus = "Dialogs/LoginDialogContent/MobileAuth/AwaitingAuthMessage".GetLocalized() ??
                                "{Invalid Resource}";
                break;
            }
            case 803:
            {
                CurrentStatus = "Dialogs/LoginDialogContent/MobileAuth/AuthedMessage".GetLocalized() ??
                                "{Invalid Resource}";
                _authenticatedAction.Invoke();
                break;
            }
        }
    }
}