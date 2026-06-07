using Cirrus.Network;
using Cirrus.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Copycat;
using Microsoft.UI.Xaml;

namespace Cirrus.ViewModels;

public partial class ProxyLoginDialogContentViewModel : ViewModel
{
    [ObservableProperty] public partial int Port { get; set; } = -1;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsReady))]
    public partial string State { get; set; }

    [ObservableProperty] public partial string UserNickname { get; set; } = string.Empty;

    public bool IsReady => State == "Ready";

    private readonly CopycatProxy? _proxy;
    private readonly SemaphoreSlim _verificationSemaphore = new(1, 1);

    public override string ViewIdentifier => string.Empty;
    
    public ProxyLoginDialogContentViewModel()
    {
        try
        {
            Client.Cookies.Clear();
            Client.UserCredentials = new();
            State = "Preparing";
            _proxy = new();
            _proxy.HeadersCaptured += ProxyOnHeadersCaptured;
            _proxy.Error += ProxyOnError;
        }
        catch
        {
            State = "Error";
        }
    }

    private void ProxyOnError(object? sender, Exception _)
    {
        DispatcherQueue!.TryEnqueue(() =>
        {
            State = "Error";
        });
        if (_proxy is null) return;
        _proxy.HeadersCaptured -= ProxyOnHeadersCaptured;
        _proxy.Error -= ProxyOnError;
    }

    private async void ProxyOnHeadersCaptured(object? sender, HeadersCapturedEventArgs e)
    {
        if (_proxy is null) return;
        if (!await _verificationSemaphore.WaitAsync(0)) return;
        try
        {
            Client.UserCredentials = e.Headers;
            DispatcherQueue!.TryEnqueue(() =>
            {
                State = "Verifying";
            });
            if (await Client.Account.GetCurrentUserAsync() is not { Profile: { } profile }) return;
            DispatcherQueue!.TryEnqueue(() => 
            {
                UserNickname = profile.Nickname;
                State = "Ready";
            });
            _proxy.HeadersCaptured -= ProxyOnHeadersCaptured;
            _proxy.Error -= ProxyOnError;
        }
        catch
        {
            _proxy.HeadersCaptured -= ProxyOnHeadersCaptured;
            _proxy.Error -= ProxyOnError;
        }
        finally
        {
            _verificationSemaphore.Release();
        }
    }

    public async Task Complete()
    {
        if (_proxy is null) return;
        _proxy.Terminate();
        if (Application.Current is not App { CurrentAccount: { } account }) return;
        await account.ReloadAsync();
    }

    public async Task Cancel()
    {
        if (_proxy is null) return;
        _proxy.Terminate();
        if (Application.Current is not App { CurrentAccount: { } account }) return;
        await account.LogOutAsync();
    }

    public override Task LoadDataAsync()
    {
        return Task.CompletedTask;
    }

    [RelayCommand]
    private void StartProxy()
    {
        if (_proxy is null) return;
        DispatcherQueue!.TryEnqueue(() =>
        {
            Port = _proxy.Start();
            State = "Capturing";
        });
    }
}