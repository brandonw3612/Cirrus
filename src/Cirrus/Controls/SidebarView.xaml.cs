using System.Reactive.Linq;
using System.Reactive.Subjects;
using Cirrus.Base.Services;
using Cirrus.Behaviors;
using Cirrus.Extensions;
using Cirrus.Models.Business.Playback;
using Cirrus.Network;
using Cirrus.Playback.PlaybackQueueProviders;
using Cirrus.Playback.Primitives;
using Cirrus.Utilities;
using Cirrus.ViewModels;
using Cirrus.Views;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;

namespace Cirrus.Controls;

[ContentProperty(Name = nameof(Child))]
public sealed partial class SidebarView
{
    public SidebarViewModel ViewModel { get; }
    
    private readonly Subject<double> _pointerSnapSubject;
    private readonly IDisposable _pointerSnapSubscription;

    public UIElement? Child
    {
        get => (UIElement?)GetValue(ChildProperty);
        set => SetValue(ChildProperty, value);
    }
    public static readonly DependencyProperty ChildProperty = DependencyProperty.Register(nameof(Child),
        typeof(UIElement), typeof(SidebarView), new PropertyMetadata(null));

    public NavigationStack NavigationStack { get; } = new();

    public SidebarView()
    {
        InitializeComponent();
        ViewModel = new((value) =>
        {
            VisualStateManager.GoToState(this, value ? "FullSized" : "CompactClosed", true);
        });
        _pointerSnapSubject = new();
        _pointerSnapSubscription = _pointerSnapSubject
            .Select(x => new
            {
                X = x,
                IsOpen = x is >= 0d and <= 8d
            })
            .Publish(shared => shared
                .DistinctUntilChanged(s => s.IsOpen)
                .Select(state => state.IsOpen
                    ? Observable
                        .Timer(TimeSpan.FromMilliseconds(300))
                        .WithLatestFrom(shared.StartWith(state), (_, latest) => latest.X)
                    : Observable.Empty<double>()
                )
                .Switch()
            )
            .ObserveOn(DispatcherQueue)
            .Subscribe(_ =>
            {
                VisualStateManager.GoToState(this, "CompactOpen", true);
            });
        this.AttachEventTriggerBehaviorToCommand<FrameworkElement, UnloadedEventTriggerBehavior>(UnloadedCommand);
    }
    
    public void SetCurrentViewIdentifier(string viewIdentifier) => ViewModel.CurrentFrameContentIdentifier = viewIdentifier;

    [RelayCommand]
    private void OnUnloaded()
    {
        _pointerSnapSubscription.Dispose();
        _pointerSnapSubject.Dispose();
    }
    
    [RelayCommand]
    private void OnSizeChanged(SizeChangedEventArgs args)
    {
        ViewModel.IsFullSizeAvailable = args.NewSize.Width >= 1000d;
        if (!ViewModel.IsFullSizeToggled) return;
        VisualStateManager.GoToState(this, ViewModel.IsFullSizeAvailable ? "FullSized" : "CompactClosed", true);
    }

    [RelayCommand]
    private void OnPaneClosed() => VisualStateManager.GoToState(this, "CompactClosed", true);

    [RelayCommand]
    private void SidebarPanePointMoved(PointerRoutedEventArgs args)
    {
        if (ViewModel is { IsFullSizeAvailable: true, IsFullSizeToggled: true } ||
            args.Pointer.PointerDeviceType is PointerDeviceType.Touch) return;
        _pointerSnapSubject.OnNext(args.GetCurrentPoint(this).Position.X);
    }
    
    [RelayCommand]
    private void SidebarPanePointExited(PointerRoutedEventArgs args)
    {
        if (ViewModel is { IsFullSizeAvailable: true, IsFullSizeToggled: true } ||
            args.Pointer.PointerDeviceType is PointerDeviceType.Touch) return;
        _pointerSnapSubject.OnNext(-1d);
    }

    [RelayCommand]
    private void SidebarPaneTapped(TappedRoutedEventArgs args)
    {
        if (ViewModel is { IsFullSizeAvailable: true, IsFullSizeToggled: true } ||
            args.PointerDeviceType is not PointerDeviceType.Touch) return;
        VisualStateManager.GoToState(this, "CompactOpen", true);
    }

    [RelayCommand]
    private void ToggleFullSized() => ViewModel.IsFullSizeToggled = !ViewModel.IsFullSizeToggled;

    [RelayCommand]
    private void SidebarNavigate(string destinationViewIdentifier)
    {
        if (MainWindow.Current is not { } window) return;
        switch (destinationViewIdentifier)
        {
            case "Search": window.Navigate(typeof(SearchView)); break;
            case "Browse": window.Navigate(typeof(BrowseView)); break;
            case "Library": window.Navigate(typeof(LibraryView)); break;
            case "CloudDrive": window.Navigate(typeof(CloudDriveView)); break;
            // case "History": window.Navigate(typeof(HistoryView)); break;
        }
    }

    [RelayCommand]
    private async Task StartPersonalStation()
    {
        if (ServicesProvider.GetService<IPlaybackService<ulong>>() is not { } playbackService) return;
        playbackService.QueueProvider = await StationQueueProvider<ulong>.CreateAsync(GetRadioTracks);
    }

    private async Task<IEnumerable<IAudioTrack<ulong>>> GetRadioTracks()
    {
        var response = await Client.Account.GetRadioTracks();
        return response.Tracks.Select(t => t.ToBusinessModel());
    }

    [RelayCommand]
    private void TogglePane()
    {
        if (ViewModel.IsFullSizeAvailable)
        {
            ViewModel.IsFullSizeToggled = !ViewModel.IsFullSizeToggled;
            return;
        }
        VisualStateManager.GoToState(this, "CompactOpen", true);
    }
}