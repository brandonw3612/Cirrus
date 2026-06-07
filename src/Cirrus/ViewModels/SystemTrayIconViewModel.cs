using Windows.ApplicationModel;
using Cirrus.Base.Services;
using Cirrus.LiveModels;
using Cirrus.Playback.Extensions;
using Cirrus.Playback.Primitives;
using Cirrus.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using H.NotifyIcon;
using Microsoft.Extensions.DependencyInjection;

namespace Cirrus.ViewModels;

internal partial class SystemTrayIconViewModel : ObservableObject
{
    public string ApplicationName => Package.Current.DisplayName;

    public PlaybackServiceBridge PSB { get; } = new();

    [RelayCommand]
    private void RestoreMainWindow()
    {
        if (MainWindow.Current is not { } window) return;
        window.Show();
        window.Activate();
    }

    [RelayCommand]
    private Task? PreviousTrack() => ServicesProvider.Current.GetService<IPlaybackService<ulong>>()?.TryPreviousAsync();
    
    [RelayCommand]
    private Task? PlayPause() => ServicesProvider.Current.GetService<IPlaybackService<ulong>>()?.PlayPauseAsync();
    
    [RelayCommand]
    private Task? NextTrack() => ServicesProvider.Current.GetService<IPlaybackService<ulong>>()?.TryNextAsync();

    [RelayCommand]
    private void ExitApplication()
    {
        Environment.Exit(0);
    }
}
