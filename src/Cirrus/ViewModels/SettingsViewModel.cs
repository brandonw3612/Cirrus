using Windows.ApplicationModel;
using Windows.System;
using Cirrus.Base.Services;
using Cirrus.Commanding.Primitives;
using Cirrus.Dialogs;
using Cirrus.LiveModels;
using Cirrus.Models.Business.Appearance;
using Cirrus.Models.Shared.Track;
using Cirrus.Models.Shared.User;
using Cirrus.Playback.Primitives;
using Cirrus.Primitives;
using Cirrus.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using AppInstance = Microsoft.Windows.AppLifecycle.AppInstance;

namespace Cirrus.ViewModels;

public partial class SettingsViewModel : ViewModel
{
    public override string ViewIdentifier => "Settings";

    private UserPreferenceService? _userPreferenceService;
    private IPlaybackService<ulong>? _playbackService;

    public List<LocalizedText<DisplayTheme>> AvailableThemes { get; } =
    [
        new("Views/SettingsView/ThemeName/Default") { Attachment = DisplayTheme.System },
        new("Views/SettingsView/ThemeName/Light") { Attachment = DisplayTheme.Light },
        new("Views/SettingsView/ThemeName/Dark") { Attachment = DisplayTheme.Dark }
    ];

    private DisplayTheme ApplicationTheme
    {
        get => _userPreferenceService!.Appearance.DisplayTheme;
        set
        {
            _userPreferenceService!.Appearance.DisplayTheme = value;
            if (MainWindow.Current is not { } window) return;
            window.UpdateTheme(value);
        }
    }

    public LocalizedText ApplicationThemeBound
    {
        get => AvailableThemes.Find(t => t.Attachment == ApplicationTheme) ?? AvailableThemes[0];
        set => ApplicationTheme = value.Attachment as DisplayTheme? ?? DisplayTheme.System;
    }

    public List<StaticText<string>> AvailableLanguages { get; } =
    [
        new("Views/SettingsView/LanguageName/Default".GetLocalized()!),
        new("简体中文") { Attachment = "zh-Hans" },
        new("繁體中文") { Attachment = "zh-Hant" },
        new("English (United States)") { Attachment = "en-US" }
    ];

    private string? ApplicationLanguage
    {
        get => _userPreferenceService!.General.DisplayLanguage;
        set
        {
            _userPreferenceService!.General.DisplayLanguage = value;
            RequestRestart("MessageBoxes/RequestRestart/LanguageSwitch/Content".GetLocalized() ?? "{Invalid Resource}");
        }
    }

    public StaticText<string> ApplicationLanguageBound
    {
        get => AvailableLanguages.Find(l => l.Attachment == ApplicationLanguage) ?? AvailableLanguages[0];
        set => ApplicationLanguage = value.Attachment;
    }

    public List<StaticText<FontFamily>> AvailableFonts { get; } = [];

    private string? DisplayFont
    {
        get => _userPreferenceService!.Appearance.DisplayFont;
        set
        {
            _userPreferenceService!.Appearance.DisplayFont = value;
            RequestRestart("MessageBoxes/RequestRestart/FontSwitch/Content".GetLocalized() ?? "{Invalid Resource}");
        }
    }

    public StaticText<FontFamily> DisplayFontBound
    {
        get => AvailableFonts.FirstOrDefault(f => f.Attachment?.Source == DisplayFont) ?? AvailableFonts[0];
        set => DisplayFont = value.Attachment?.Source;
    }

    public bool IsBatterySaverEnabled
    {
        get => _userPreferenceService!.General.IsBatterySaverEnabled;
        set
        {
            _userPreferenceService!.General.IsBatterySaverEnabled = value;
            // TODO: Apply directly.
            OnPropertyChanged();
        }
    }

    public bool IsBxbLyricsEnabled
    {
        get => _userPreferenceService!.Appearance.IsBxbLyricsEnabled;
        set => _userPreferenceService!.Appearance.IsBxbLyricsEnabled = value;
    }

    public bool IsDynaBackEnabled
    {
        get => _userPreferenceService!.Appearance.IsDynaBackEnabled;
        set => _userPreferenceService!.Appearance.IsDynaBackEnabled = value;
    }

    public bool RunsInBackground
    {
        get => _userPreferenceService!.General.RunsInBackground;
        set
        {
            if (_userPreferenceService!.General.RunsInBackground == value) return;
            _userPreferenceService!.General.RunsInBackground = value;
            if (MainWindow.Current is not { } window) return;
            window.UpdateRunsInBackground(value);
        }
    }

    [ObservableProperty] public partial bool IsHotLyricInstalled { get; set; }

    public bool IsHotLyricAutoLaunched
    {
        get => _userPreferenceService!.General.AutoLaunchesHotLyric;
        set
        {
            _userPreferenceService!.General.AutoLaunchesHotLyric = value;
            if (value) LaunchHotLyric();
        }
    }

    public List<LocalizedText> AvailableAudioQualities { get; } =
    [
        new("Views/SettingsView/AudioQuality/Standard")
            { Attachment = AudioQuality.Standard },

        new("Views/SettingsView/AudioQuality/Higher")
            { Attachment = AudioQuality.Higher }
    ];

    private AudioQuality PreferredAudioQuality
    {
        get => _userPreferenceService!.Playback.AudioQuality;
        set => _userPreferenceService!.Playback.AudioQuality = value;
    }

    public LocalizedText PreferredAudioQualityBound
    {
        get => AvailableAudioQualities.Find(q => PreferredAudioQuality.Equals(q.Attachment)) ??
               AvailableAudioQualities[1];
        set => PreferredAudioQuality = value.Attachment as AudioQuality? ?? AudioQuality.Higher;
    }

    public bool IsDirectSwitchEnabled
    {
        get => _playbackService!.IsDirectSwitchEnabled;
        set => _playbackService!.IsDirectSwitchEnabled = value;
    }

    public bool IsAudioCrossfadeEnabled
    {
        get => _playbackService!.IsAudioCrossfadeEnabled;
        set
        {
            if (_playbackService!.IsAudioCrossfadeEnabled == value) return;
            _playbackService!.IsAudioCrossfadeEnabled = value;
            OnPropertyChanged();
        }
    }

    public double AudioCrossfadeDuration
    {
        get => _playbackService!.AudioCrossfadeDuration;
        set => _playbackService!.AudioCrossfadeDuration = value;
    }

    public bool IsIpFixOn
    {
        get => Network.Client.IpFix is not null;
        set => Network.Client.IpFix = value ? "36.36.12.12" : null;
    }

    public bool IsHttpFallbackEnabled
    {
        get => _userPreferenceService!.Network.IsHttpFallbackEnabled;
        set
        {
            _userPreferenceService!.Network.IsHttpFallbackEnabled = value;
            Network.Api.Constants.Protocol = value ? "http" : "https";
        }
    }

    public string ApplicationName => Package.Current.DisplayName;
    public string FrameworkVersion => Environment.Version.ToString();
    public string ApplicationVersion => Package.Current.Id.Version.ToFormattedString();
    public string InstalledDate => Package.Current.InstalledDate.ToLocalTime().ToString("yyyy-MM-dd");

    public override async Task LoadDataAsync()
    {
        if (ServicesProvider.Current.GetService(typeof(UserPreferenceService)) is not UserPreferenceService service)
        {
            throw new Exception("Internal fatal error: Could not load preference service.");
        }

        _userPreferenceService = service;
        if (ServicesProvider.Current.GetService(typeof(IPlaybackService<ulong>)) is not IPlaybackService<ulong>
            playbackService)
        {
            throw new Exception("Internal fatal error: Could not load playback service.");
        }

        _playbackService = playbackService;

        var systemFonts = Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies()
            .Where(static f => f is { Length: > 0 }).Distinct().OrderBy(static f => f).ToArray();
        var defaultFont = systemFonts.Contains("Segoe UI Variable") ? "Segoe UI Variable" : "Segoe UI";
        AvailableFonts.Clear();
        AvailableFonts.Add(new("Views/SettingsView/Appearance/FontName/Default".GetLocalized()!)
            { Attachment = new(defaultFont) });
        AvailableFonts.AddRange(systemFonts.Select(f => new StaticText<FontFamily>(f) { Attachment = new(f) }));

        IsHotLyricInstalled =
            await Launcher.QueryUriSupportAsync(new Uri("hot-lyric://"), LaunchQuerySupportType.Uri) is
                LaunchQuerySupportStatus.Available;

        if (AvailableAudioQualities.Count > 2)
            AvailableAudioQualities.RemoveRange(2, AvailableAudioQualities.Count - 2);
        if (Application.Current as App is { CurrentAccount: { IsLoggedIn: true, MembershipStatus: { } status } })
        {
            if (status.MembershipType.HasAccessToLosslessAudio())
            {
                AvailableAudioQualities.Add(new("Views/SettingsView/AudioQuality/Lossless")
                    { Attachment = AudioQuality.Lossless });
                AvailableAudioQualities.Add(new("Views/SettingsView/AudioQuality/HiRes")
                    { Attachment = AudioQuality.HiRes });
                AvailableAudioQualities.Add(new("Views/SettingsView/AudioQuality/Spatial")
                    { Attachment = AudioQuality.Spatial });
            }

            if (status.MembershipType.HasAccessToMasterAudio())
            {
                AvailableAudioQualities.Add(new("Views/SettingsView/AudioQuality/Surround")
                    { Attachment = AudioQuality.Surround });
                AvailableAudioQualities.Add(new("Views/SettingsView/AudioQuality/Master")
                    { Attachment = AudioQuality.Master });
            }
        }
    }

    private static RelayCommandEx<FrameworkElement, RoutedEventArgs>? _showAttachedFlyoutCommand;

    public RelayCommandEx<FrameworkElement, RoutedEventArgs> ShowAttachedFlyoutCommand => _showAttachedFlyoutCommand ??=
        new((sender, _) => { FlyoutBase.GetAttachedFlyout(sender).ShowAt(sender); });

    [RelayCommand]
    private async Task ShowKeyboardShortcutsCustomizationDialog()
    {
        if (MainWindow.Current is not { } window) return;
        ContentDialog dialog = new()
        {
            Title = "Dialogs/KeyboardShortcutsCustomizationDialog/Title".GetLocalized(),
            Content = new KeyboardShortcutsCustomizationDialogContent(),
            PrimaryButtonText = "Controls/Buttons/Save/Content".GetLocalized(),
            CloseButtonText = "Controls/Buttons/Cancel/Content".GetLocalized(),
            DefaultButton = ContentDialogButton.Primary,
        };

        static async void OnDialogOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            if (sender.Content is not KeyboardShortcutsCustomizationDialogContent content) return;
            await content.LoadConfigAsync();
            sender.Opened -= OnDialogOpened;
        }

        static async void OnPrimaryButtonClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            if (sender.Content is not KeyboardShortcutsCustomizationDialogContent content) return;
            if (!content.TryExportConfig(out var config)) return;
            if (MainWindow.Current is { } w) w.ApplyKeyboardHandler(config);
            await config.SaveAsync();
            sender.PrimaryButtonClick -= OnPrimaryButtonClicked;
            sender.Hide();
        }

        dialog.PrimaryButtonClick += OnPrimaryButtonClicked;
        dialog.Opened += OnDialogOpened;
        await window.DialogController.ShowCustomContentDialogAsync(dialog);
    }

    [RelayCommand]
    private async Task LaunchStoreForHotLyric()
    {
        await Launcher.LaunchUriAsync(new("ms-windows-store://pdp/?ProductId=9MXFFHVQVBV9&mode=mini"));
    }

    [RelayCommand]
    private async Task ShowNetworkProxyConfigDialog()
    {
        if (MainWindow.Current is not { } window) return;
        var content = new NetworkProxyConfigDialogContent();
        ContentDialog dialog = new()
        {
            Title = "Dialogs/NetworkProxyConfigDialog/Title".GetLocalized(),
            Content = content,
            CloseButtonText = "Controls/Buttons/Save/Content".GetLocalized(),
            CloseButtonCommand = new RelayCommand(content.ViewModel.UpdateSettings),
            DefaultButton = ContentDialogButton.Close,
        };
        await window.DialogController.ShowCustomContentDialogAsync(dialog);
    }

    [RelayCommand]
    private void GoToUpdatesAndExperiences()
    {
        if (MainWindow.Current is not { } window) return;
        window.Navigate(typeof(SoftwareUpdateView));
    }

    [RelayCommand]
    private async Task ViewOnGitHub()
    {
        await Launcher.LaunchUriAsync(new("https://github.com/brandonw3612/Cirrus"));
    }

    private static async void LaunchHotLyric()
    {
        var queryResults = await Launcher.QueryUriSupportAsync(new("hot-lyric://"), LaunchQuerySupportType.Uri);
        if (queryResults is not LaunchQuerySupportStatus.Available) return;
        await Launcher.LaunchUriAsync(new("hot-lyric:///?from=17588BrandonWong.Cirrus_13cqesaq5mk46"));
    }

    private static async void RequestRestart(string content)
    {
        if (MainWindow.Current is not { } window) return;
        await window.DialogController.ShowMessageBoxAsync(
            "MessageBoxes/RequestRestart/Title".GetLocalized() ?? "{Invalid Resource}",
            content,
            "MessageBoxes/RequestRestart/PrimaryButtonText".GetLocalized() ?? "{Invalid Resource}",
            "MessageBoxes/RequestRestart/CancelButtonText".GetLocalized() ?? "{Invalid Resource}",
            () => { AppInstance.Restart(string.Empty); }
        );
    }
}