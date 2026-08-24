using Cirrus.Base.Services;
using Cirrus.Controls;
using Cirrus.Enums;
using Cirrus.Models.Business.Appearance;
using Cirrus.Utilities;
using Cirrus.ViewModels;
using CommunityToolkit.Mvvm.Input;
using H.NotifyIcon;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Color = Windows.UI.Color;

namespace Cirrus.Views;

public sealed partial class MainWindow
{
    public MainWindowViewModel ViewModel { get; } = new();

    public NavigationStack NavigationStack => RootSidebarView.NavigationStack;

    public new static MainWindow? Current { get; private set; }
    public ElementTheme CurrentTheme => HostFrame.ActualTheme;
    public XamlRoot XamlRoot => HostFrame.XamlRoot;
    public DialogController DialogController { get; }
    private KeyboardActionHandler? _currentHandler;

    public MainWindow()
    {
        Current = this;
        InitializeComponent();
        DialogController = new(this);
        Closed += OnClosed;
    }

    [RelayCommand]
    private async Task OnLoaded()
    {
        ApplyTitleBarStyle();
        AppWindow.SetIcon("Assets/ApplicationIcon.ico");
        AppWindow.SetTaskbarIcon("Assets/ApplicationIcon.ico");
        AppWindow.SetTitleBarIcon("Assets/ApplicationIcon.ico");
        ApplyKeyboardHandler(await KeyboardActionHandler.CreateAsync());
        if (ServicesProvider.GetService<UserPreferenceService>() is not { } userPreferenceService) return;
        UpdateTheme(userPreferenceService.Appearance.DisplayTheme);
        if (userPreferenceService.Appearance.DisplayFont is { } fontName)
        {
            HostFrame.FontFamily = new(fontName);
        }
        if (userPreferenceService.General.RunsInBackground)
        {
            RootGrid.Children.Add(new SystemTrayIcon());
        }
        PlaybackDrawer.Navigate(typeof(PlaybackControlView));
        if (Application.Current is App { CurrentAccount: { IsLoggedIn: true, UserAccount: { UserId: { } userId } } })
        {
            Navigate(typeof(UserDetailView), userId);
        }
        else
        {
            Navigate(typeof(BrowseView));
        }
    }

    public void Navigate(Type destinationViewType, object? navigationParameter = null,
        NavigationDirection direction = NavigationDirection.Forward)
    {
        TogglePlaybackDrawer(false);
        ContentFrame.Navigate(destinationViewType, navigationParameter, new SlideNavigationTransitionInfo
        {
            Effect = direction is NavigationDirection.Forward
                ? SlideNavigationTransitionEffect.FromRight
                : SlideNavigationTransitionEffect.FromLeft
        });
    }
    
    public void SetCurrentViewIdentifier(string viewIdentifier)
    {
        RootSidebarView.SetCurrentViewIdentifier(viewIdentifier);
    }

    public void UpdateTheme(DisplayTheme theme)
    {
        HostFrame.RequestedTheme = theme switch
        {
            DisplayTheme.Light => ElementTheme.Light,
            DisplayTheme.Dark => ElementTheme.Dark,
            _ => ElementTheme.Default
        };
        AppWindow.TitleBar.ButtonForegroundColor = theme switch
        {
            DisplayTheme.Light => Color.FromArgb(255, 0, 0, 0),
            DisplayTheme.Dark => Color.FromArgb(255, 255, 255, 255),
            _ => null
        };
    }

    public void ApplyKeyboardHandler(KeyboardActionHandler? handler)
    {
        _currentHandler?.Detach(HostFrame);
        _currentHandler = handler;
        handler?.Attach(HostFrame);
    }
    
    public void UpdateRunsInBackground(bool runsInBackground)
    {
        if (runsInBackground)
        {
            if (RootGrid.Children.Any(e => e is SystemTrayIcon)) return;
            RootGrid.Children.Add(new SystemTrayIcon());
            return;
        }
        var icons = RootGrid.Children.OfType<SystemTrayIcon>();
        foreach (var icon in icons)
        {
            icon.Dispose();
            RootGrid.Children.Remove(icon);
        }
    }

    public void TogglePlaybackDrawer(bool isOpen)
    {
        PlaybackDrawer.IsOpen = isOpen;
        var theme = isOpen
            ? ElementTheme.Dark
            : CurrentTheme;
        AppWindow.TitleBar.ButtonForegroundColor = theme switch
        {
            ElementTheme.Light => Color.FromArgb(255, 0, 0, 0),
            _ => Color.FromArgb(255, 255, 255, 255),
        };
    }

    public void ShowNotification(string content, int durationInMilliseconds = 0,
        InfoBarSeverity severity = InfoBarSeverity.Informational, string? title = null)
    {
        ViewModel.NotificationSeverity = severity;
        NotificationQueue.Show(content, durationInMilliseconds, title);
    }

    private void OnClosed(object sender, WindowEventArgs args)
    {
        if (ServicesProvider.GetService<UserPreferenceService>() is not { General.RunsInBackground: true }) return;
        this.Hide();
        args.Handled = true;
    }

    private void ApplyTitleBarStyle()
    {
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(TitleBar);
        var titleBar = AppWindow.TitleBar;
        titleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
    }
}