using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Cirrus.Base.Services.Abstract;
using Cirrus.Models.Business.Appearance;
using Cirrus.Models.Business.Network;
using Cirrus.Models.Business.Playback;
using Cirrus.Models.Shared.Track;
using SerializationContext = Cirrus.Base.Serialization.SerializationContext;

namespace Cirrus.Base.Services;

/// <summary>
/// Service for accessing user preferences.
/// </summary>
[RegisterSingleton]
public class UserPreferenceService
{
    /// <summary>
    /// Constructs the service from the IoC container.
    /// </summary>
    /// <param name="preferenceAccessService">Preference access service from IoC container.</param>
    public UserPreferenceService(IPreferenceAccessService preferenceAccessService)
    {
        // Initialize preference containers.
        Account = new(preferenceAccessService);
        Appearance = new(preferenceAccessService);
        General = new(preferenceAccessService);
        Network = new(preferenceAccessService);
        Playback = new(preferenceAccessService);
    }
    
    /// <summary>
    /// Container for preferences related to the user account.
    /// </summary>
    public AccountPreferenceContainer Account { get; }
    
    /// <summary>
    /// Container for preferences related to appearance.
    /// </summary>
    public AppearancePreferenceContainer Appearance { get; }
    
    /// <summary>
    /// Container for general preferences.
    /// </summary>
    public GeneralPreferenceContainer General { get; }
    
    /// <summary>
    /// Container for preferences related to networking.
    /// </summary>
    public NetworkPreferenceContainer Network { get; }
    
    /// <summary>
    /// Container for preferences related to playback.
    /// </summary>
    public PlaybackPreferenceContainer Playback { get; }
    
    /// <summary>
    /// Definition for preference container related to the user account.
    /// </summary>
    public class AccountPreferenceContainer : UserPreferenceContainer
    {
        /// <summary>
        /// Constructs the container with preference access service from IoC container.
        /// </summary>
        /// <param name="preferenceAccessService">Preference access service from IoC container.</param>
        internal AccountPreferenceContainer(IPreferenceAccessService preferenceAccessService) : base(preferenceAccessService)
        {
            // No further action.
        }

        #region UserCookiesJson

        /// <summary>
        /// User cookies used by the network client. Null if not logged in.
        /// </summary>
        public CookieCollection? UserCookies
        {
            get
            {
                var json = _userCookiesPreferenceEntry.GetValue(PreferenceAccessService);
                if (json is null ||
                    JsonSerializer.Deserialize<List<Cookie>>(json, SerializationContext.Default.ListCookie) is not
                        { } cookies) return null;
                var cookieCollection = new CookieCollection();
                cookies.ForEach(cookieCollection.Add);
                return cookieCollection;
            }
            set
            {
                if (value is not {Count: > 0})
                {
                    _userCookiesPreferenceEntry.SetValue(PreferenceAccessService, null);
                    return;
                }
                _userCookiesPreferenceEntry.SetValue(PreferenceAccessService,
                    JsonSerializer.Serialize<List<Cookie>>(value.ToList(), SerializationContext.Default.ListCookie));
            }
        }
        
        /// <summary>
        /// Preference entry for <see cref="UserCookies"/>.
        /// </summary>
        private readonly UserPreferenceEntry<string?> _userCookiesPreferenceEntry = new()
        {
            PreferencePath = $"{nameof(Account)}/{nameof(UserCookies)}",
            DefaultValue = null,
            IsLarge = true
        };

        public JsonObject? UserCredentials
        {
            get
            {
                var json = _userCredentialsPreferenceEntry.GetValue(PreferenceAccessService);
                return json is { Length: > 0 }
                    ? JsonNode.Parse(json)?.AsObject()
                    : null;
            }
            set
            {
                if (value is null)
                {
                    _userCredentialsPreferenceEntry.SetValue(PreferenceAccessService, null);
                    return;
                }
                _userCredentialsPreferenceEntry.SetValue(PreferenceAccessService, value.ToJsonString());
            }
        }

        private readonly UserPreferenceEntry<string?> _userCredentialsPreferenceEntry = new()
        {
            PreferencePath = $"{nameof(Account)}/{nameof(UserCredentials)}",
            DefaultValue = null,
            IsLarge = true
        };

        #endregion

        #region LastFmSessionJson

        /// <summary>
        /// JSON string of the Last.fm session. Null if not connected.
        /// </summary>
        public string? LastFmSessionJson
        {
            get => _lastFmSessionJsonPreferenceEntry.GetValue(PreferenceAccessService);
            set => _lastFmSessionJsonPreferenceEntry.SetValue(PreferenceAccessService, value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="LastFmSessionJson"/>.
        /// </summary>
        private readonly UserPreferenceEntry<string?> _lastFmSessionJsonPreferenceEntry = new()
        {
            PreferencePath = $"{nameof(Account)}/{nameof(LastFmSessionJson)}",
            DefaultValue = null,
            IsLarge = true
        };

        #endregion

        #region Operations

        public void RemoveCurrentUser()
        {
            //_userCookiesPreferenceEntry.RemoveValue(PreferenceAccessService);
            //_userIdPreferenceEntry.RemoveValue(PreferenceAccessService);
            //_userNicknamePreferenceEntry.RemoveValue(PreferenceAccessService);
            //_userAvatarUrlPreferenceEntry.RemoveValue(PreferenceAccessService);
            //_isUserPremiumPreferenceEntry.RemoveValue(PreferenceAccessService);
        }

        #endregion
    }

    /// <summary>
    /// Definition for preference container related to appearance.
    /// </summary>
    public class AppearancePreferenceContainer : UserPreferenceContainer
    {
        /// <summary>
        /// Constructs the container with preference access service from IoC container.
        /// </summary>
        /// <param name="preferenceAccessService">Preference access service from IoC container.</param>
        internal AppearancePreferenceContainer(IPreferenceAccessService preferenceAccessService) : base(preferenceAccessService)
        {
            // No further action.
        }
        
        #region PlayerViewMode

        /// <summary>
        /// View mode of the player.
        /// </summary>
        public PlaybackControlViewMode PlaybackControlViewMode
        {
            get => (PlaybackControlViewMode)_playerViewModePreferenceEntry.GetValue(PreferenceAccessService);
            set => _playerViewModePreferenceEntry.SetValue(PreferenceAccessService, (int)value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="PlaybackControlViewMode"/>.
        /// </summary>
        private readonly UserPreferenceEntry<int> _playerViewModePreferenceEntry = new()
        {
            PreferencePath = $"{nameof(Appearance)}/{nameof(PlaybackControlViewMode)}",
            DefaultValue = 0
        };

        #endregion

        #region IsLyricsTranslationVisible

        /// <summary>
        /// Whether lyrics translation is visible.
        /// </summary>
        public bool IsLyricsTranslationVisible
        {
            get => _isLyricsTranslationVisiblePreferenceEntry.GetValue(PreferenceAccessService);
            set => _isLyricsTranslationVisiblePreferenceEntry.SetValue(PreferenceAccessService, value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="IsLyricsTranslationVisible"/>.
        /// </summary>
        private readonly UserPreferenceEntry<bool> _isLyricsTranslationVisiblePreferenceEntry = new()
        {
            PreferencePath = $"{nameof(Appearance)}/{nameof(IsLyricsTranslationVisible)}",
            DefaultValue = false
        };

        #endregion

        #region LyricsBaseFontSize

        /// <summary>
        /// Base font size of the lyrics.
        /// </summary>
        public int LyricsBaseFontSize
        {
            get => _lyricsBaseFontSizePreferenceEntry.GetValue(PreferenceAccessService);
            set => _lyricsBaseFontSizePreferenceEntry.SetValue(PreferenceAccessService, value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="LyricsBaseFontSize"/>.
        /// </summary>
        private readonly UserPreferenceEntry<int> _lyricsBaseFontSizePreferenceEntry = new()
        {
            PreferencePath = $"{nameof(Appearance)}/{nameof(LyricsBaseFontSize)}",
            DefaultValue = 24
        };

        #endregion

        #region IsVerbatimLyricsEnabled

        /// <summary>
        /// Whether beat-by-beat lyrics are enabled.
        /// </summary>
        public bool IsBxbLyricsEnabled
        {
            get => _isBxbLyricsEnabledPreferenceEntry.GetValue(PreferenceAccessService);
            set => _isBxbLyricsEnabledPreferenceEntry.SetValue(PreferenceAccessService, value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="IsBxbLyricsEnabled"/>.
        /// </summary>
        private readonly UserPreferenceEntry<bool> _isBxbLyricsEnabledPreferenceEntry = new()
        {
            PreferencePath = $"{nameof(Appearance)}/{nameof(IsBxbLyricsEnabled)}",
            DefaultValue = true
        };

        #endregion

        #region DisplayFont

        /// <summary>
        /// Display font of the application.
        /// </summary>
        public string? DisplayFont
        {
            get => _displayFontPreferenceEntry.GetValue(PreferenceAccessService);
            set => _displayFontPreferenceEntry.SetValue(PreferenceAccessService, value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="DisplayFont"/>.
        /// </summary>
        private readonly UserPreferenceEntry<string?> _displayFontPreferenceEntry = new()
        {
            PreferencePath = $"{nameof(Appearance)}/{nameof(DisplayFont)}",
            DefaultValue = null
        };

        #endregion

        #region DisplayTheme

        /// <summary>
        /// Display theme of the application.
        /// </summary>
        public DisplayTheme DisplayTheme
        {
            get => (DisplayTheme)_displayThemePreferenceEntry.GetValue(PreferenceAccessService);
            set => _displayThemePreferenceEntry.SetValue(PreferenceAccessService, (int)value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="DisplayTheme"/>.
        /// </summary>
        private readonly UserPreferenceEntry<int> _displayThemePreferenceEntry = new()
        {
            PreferencePath = $"{nameof(Appearance)}/{nameof(DisplayTheme)}",
            DefaultValue = 0
        };

        #endregion

        #region IsDynaBackEnabled

        /// <summary>
        /// Whether dynamic background of the playback control view is enabled.
        /// </summary>
        public bool IsDynaBackEnabled
        {
            get => _isDynaBackEnabledPreferenceEntry.GetValue(PreferenceAccessService);
            set => _isDynaBackEnabledPreferenceEntry.SetValue(PreferenceAccessService, value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="IsDynaBackEnabled"/>.
        /// </summary>
        private readonly UserPreferenceEntry<bool> _isDynaBackEnabledPreferenceEntry = new()
        {
            PreferencePath = $"{nameof(Appearance)}/{nameof(IsDynaBackEnabled)}",
            DefaultValue = true
        };

        #endregion

        #region IsSidebarOpen

        /// <summary>
        /// Whether the application sidebar is open.
        /// </summary>
        public bool IsSidebarOpen
        {
            get => _isSidebarOpenPreferenceEntry.GetValue(PreferenceAccessService);
            set => _isSidebarOpenPreferenceEntry.SetValue(PreferenceAccessService, value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="IsSidebarOpen"/>.
        /// </summary>
        private readonly UserPreferenceEntry<bool> _isSidebarOpenPreferenceEntry = new()
        {
            PreferencePath = $"{nameof(Appearance)}/{nameof(IsSidebarOpen)}",
            DefaultValue = false
        };

        #endregion
    }

    /// <summary>
    /// Definition for general preference container.
    /// </summary>
    public class GeneralPreferenceContainer : UserPreferenceContainer
    {
        /// <summary>
        /// Constructs the container with preference access service from IoC container.
        /// </summary>
        /// <param name="preferenceAccessService">Preference access service from IoC container.</param>
        internal GeneralPreferenceContainer(IPreferenceAccessService preferenceAccessService) : base(preferenceAccessService)
        {
            // No further action.
        }

        #region AutoLaunchesHotLyric

        /// <summary>
        /// Whether we automatically launches hot lyric on startup.
        /// </summary>
        public bool AutoLaunchesHotLyric
        {
            get => _autoLaunchesHotLyricPreferenceEntry.GetValue(PreferenceAccessService);
            set => _autoLaunchesHotLyricPreferenceEntry.SetValue(PreferenceAccessService, value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="AutoLaunchesHotLyric"/>.
        /// </summary>
        private readonly UserPreferenceEntry<bool> _autoLaunchesHotLyricPreferenceEntry = new()
        {
            PreferencePath = $"{nameof(General)}/{nameof(AutoLaunchesHotLyric)}",
            DefaultValue = true
        };

        #endregion

        #region RunsInBackground
        
        /// <summary>
        /// Whether the application runs in the background.
        /// </summary>
        public bool RunsInBackground
        {
            get => _runsInBackgroundPreferenceEntry.GetValue(PreferenceAccessService);
            set => _runsInBackgroundPreferenceEntry.SetValue(PreferenceAccessService, value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="RunsInBackground"/>.
        /// </summary>
        private readonly UserPreferenceEntry<bool> _runsInBackgroundPreferenceEntry = new()
        {
            PreferencePath = $"{nameof(General)}/{nameof(RunsInBackground)}",
            DefaultValue = false
        };

        #endregion

        #region DisplayLanguage

        /// <summary>
        /// Display language of the application.
        /// </summary>
        public string? DisplayLanguage
        {
            get => _displayLanguagePreferenceEntry.GetValue(PreferenceAccessService);
            set => _displayLanguagePreferenceEntry.SetValue(PreferenceAccessService, value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="DisplayLanguage"/>.
        /// </summary>
        private readonly UserPreferenceEntry<string?> _displayLanguagePreferenceEntry = new()
        {
            PreferencePath = $"{nameof(General)}/{nameof(DisplayLanguage)}",
            DefaultValue = null
        };

        #endregion

        #region IsBatterySaverEnabled
        
        /// <summary>
        /// Whether battery saver is enabled.
        /// </summary>
        public bool IsBatterySaverEnabled
        {
            get => _isBatterySaverEnabledPreferenceEntry.GetValue(PreferenceAccessService);
            set => _isBatterySaverEnabledPreferenceEntry.SetValue(PreferenceAccessService, value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="IsBatterySaverEnabled"/>.
        /// </summary>
        private readonly UserPreferenceEntry<bool> _isBatterySaverEnabledPreferenceEntry = new()
        {
            PreferencePath = $"{nameof(General)}/{nameof(IsBatterySaverEnabled)}",
            DefaultValue = false
        };

        #endregion

        #region UpdateCheckedTime
        
        /// <summary>
        /// Last time we checked for updates.
        /// </summary>
        public DateTimeOffset? UpdateCheckedTime
        {
            get => _updateCheckedTimePreferenceEntry.GetValue(PreferenceAccessService);
            set => _updateCheckedTimePreferenceEntry.SetValue(PreferenceAccessService, value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="UpdateCheckedTime"/>.
        /// </summary>
        private readonly UserPreferenceEntry<DateTimeOffset?> _updateCheckedTimePreferenceEntry = new()
        {
            PreferencePath = $"{nameof(General)}/{nameof(UpdateCheckedTime)}",
            DefaultValue = null
        };

        #endregion

        #region ArePreviewBuildsEnabled

        /// <summary>
        /// Whether preview builds are enabled when checking for updates.
        /// </summary>
        public bool ArePreviewBuildsEnabled
        {
            get => _arePreviewBuildsEnabledPreferenceEntry.GetValue(PreferenceAccessService);
            set => _arePreviewBuildsEnabledPreferenceEntry.SetValue(PreferenceAccessService, value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="ArePreviewBuildsEnabled"/>.
        /// </summary>
        private readonly UserPreferenceEntry<bool> _arePreviewBuildsEnabledPreferenceEntry = new()
        {
            PreferencePath = $"{nameof(General)}/{nameof(ArePreviewBuildsEnabled)}",
            DefaultValue = false
        };

        #endregion

        #region SendDiagnosticsData
        
        /// <summary>
        /// Whether we send diagnostics data.
        /// </summary>
        public bool SendDiagnosticsData
        {
            get => _sendDiagnosticsDataPreferenceEntry.GetValue(PreferenceAccessService);
            set => _sendDiagnosticsDataPreferenceEntry.SetValue(PreferenceAccessService, value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="SendDiagnosticsData"/>.
        /// </summary>
        private readonly UserPreferenceEntry<bool> _sendDiagnosticsDataPreferenceEntry = new()
        {
            PreferencePath = $"{nameof(General)}/{nameof(SendDiagnosticsData)}",
            DefaultValue = true
        };

        #endregion
    }

    /// <summary>
    /// Definition for preference container related to networking.
    /// </summary>
    public class NetworkPreferenceContainer : UserPreferenceContainer
    {
        /// <summary>
        /// Constructs the container with preference access service from IoC container.
        /// </summary>
        /// <param name="preferenceAccessService">Preference access service from IoC container.</param>
        internal NetworkPreferenceContainer(IPreferenceAccessService preferenceAccessService) : base(preferenceAccessService)
        {
            // No further action.
        }

        #region IpFix

        /// <summary>
        /// Fixed the IP address of the application. Used outside China mainland.
        /// </summary>
        public string? IpFix
        {
            get => _ipFixPreferenceEntry.GetValue(PreferenceAccessService);
            set => _ipFixPreferenceEntry.SetValue(PreferenceAccessService, value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="IpFix"/>.
        /// </summary>
        private readonly UserPreferenceEntry<string?> _ipFixPreferenceEntry = new()
        {
            PreferencePath = $"{nameof(Network)}/{nameof(IpFix)}",
            DefaultValue = null
        };

        #endregion
        
        #region ProxyMode

        /// <summary>
        /// Proxy mode of the application.
        /// </summary>
        public ProxyMode ProxyMode
        {
            get => (ProxyMode)_proxyModePreferenceEntry.GetValue(PreferenceAccessService);
            set => _proxyModePreferenceEntry.SetValue(PreferenceAccessService, (int)value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="ProxyMode"/>.
        /// </summary>
        private readonly UserPreferenceEntry<int> _proxyModePreferenceEntry = new()
        {
            PreferencePath = $"{nameof(Network)}/{nameof(ProxyMode)}",
            DefaultValue = 0
        };

        #endregion
        
        #region CustomProxyHost
        
        /// <summary>
        /// Host of the custom proxy.
        /// </summary>
        public string? CustomProxyHost
        {
            get => _customProxyHostPreferenceEntry.GetValue(PreferenceAccessService);
            set => _customProxyHostPreferenceEntry.SetValue(PreferenceAccessService, value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="CustomProxyHost"/>.
        /// </summary>
        private readonly UserPreferenceEntry<string?> _customProxyHostPreferenceEntry = new()
        {
            PreferencePath = $"{nameof(Network)}/{nameof(CustomProxyHost)}",
            DefaultValue = null
        };

        #endregion
        
        #region CustomProxyPort
        
        /// <summary>
        /// Port of the custom proxy.
        /// </summary>
        public int? CustomProxyPort
        {
            get => _customProxyPortPreferenceEntry.GetValue(PreferenceAccessService);
            set => _customProxyPortPreferenceEntry.SetValue(PreferenceAccessService, value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="CustomProxyPort"/>.
        /// </summary>
        private readonly UserPreferenceEntry<int?> _customProxyPortPreferenceEntry = new()
        {
            PreferencePath = $"{nameof(Network)}/{nameof(CustomProxyPort)}",
            DefaultValue = null
        };
        
        #endregion

        #region IsHttpFallbackEnabled

        /// <summary>
        /// Whether HTTP fallback is enabled.
        /// </summary>
        public bool IsHttpFallbackEnabled
        {
            get => _isHttpFallbackEnabledPreferenceEntry.GetValue(PreferenceAccessService);
            set => _isHttpFallbackEnabledPreferenceEntry.SetValue(PreferenceAccessService, value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="IsHttpFallbackEnabled"/>.
        /// </summary>
        private readonly UserPreferenceEntry<bool> _isHttpFallbackEnabledPreferenceEntry = new()
        {
            PreferencePath = $"{nameof(Network)}/{nameof(IsHttpFallbackEnabled)}",
            DefaultValue = false
        };

        #endregion
    }
    
    /// <summary>
    /// Definition for preference container related to playback.
    /// </summary>
    public class PlaybackPreferenceContainer : UserPreferenceContainer
    {
        /// <summary>
        /// Constructs the container with preference access service from IoC container.
        /// </summary>
        /// <param name="preferenceAccessService">Preference access service from IoC container.</param>
        internal PlaybackPreferenceContainer(IPreferenceAccessService preferenceAccessService) : base(preferenceAccessService)
        {
            // No further action.
        }

        #region PlaybackMode

        /// <summary>
        /// Playback mode of the player.
        /// </summary>
        public PlaybackMode PlaybackMode
        {
            get => (PlaybackMode)_playbackModePreferenceEntry.GetValue(PreferenceAccessService);
            set => _playbackModePreferenceEntry.SetValue(PreferenceAccessService, (int)value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="PlaybackMode"/>.
        /// </summary>
        private readonly UserPreferenceEntry<int> _playbackModePreferenceEntry = new()
        {
            PreferencePath = $"{nameof(Playback)}/{nameof(PlaybackMode)}",
            DefaultValue = 0
        };

        #endregion

        #region Volume

        /// <summary>
        /// Volume of the player.
        /// </summary>
        public double Volume
        {
            get => _volumePreferenceEntry.GetValue(PreferenceAccessService);
            set => _volumePreferenceEntry.SetValue(PreferenceAccessService, value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="Volume"/>.
        /// </summary>
        private readonly UserPreferenceEntry<double> _volumePreferenceEntry = new()
        {
            PreferencePath = $"{nameof(Playback)}/{nameof(Volume)}",
            DefaultValue = 1d
        };

        #endregion

        #region AudioQuality

        /// <summary>
        /// Audio quality of the player.
        /// </summary>
        public AudioQuality AudioQuality
        {
            get => (AudioQuality)_audioQualityPreferenceEntry.GetValue(PreferenceAccessService);
            set => _audioQualityPreferenceEntry.SetValue(PreferenceAccessService, (int)value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="AudioQuality"/>.
        /// </summary>
        private readonly UserPreferenceEntry<int> _audioQualityPreferenceEntry = new()
        {
            PreferencePath = $"{nameof(Playback)}/{nameof(AudioQuality)}",
            DefaultValue = 1
        };

        #endregion

        #region IsDirectSwitchEnabled

        /// <summary>
        /// Whether direct-switch is enabled.
        /// </summary>
        public bool IsDirectSwitchEnabled
        {
            get => _isDirectSwitchEnabledPreferenceEntry.GetValue(PreferenceAccessService);
            set => _isDirectSwitchEnabledPreferenceEntry.SetValue(PreferenceAccessService, value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="IsDirectSwitchEnabled"/>.
        /// </summary>
        private readonly UserPreferenceEntry<bool> _isDirectSwitchEnabledPreferenceEntry = new()
        {
            PreferencePath = $"{nameof(Playback)}/{nameof(IsDirectSwitchEnabled)}",
            DefaultValue = false
        };

        #endregion

        #region IsAudioCrossfadeEnabled

        /// <summary>
        /// Whether audio cross-fade is enabled.
        /// </summary>
        public bool IsAudioCrossfadeEnabled
        {
            get => _isAudioCrossFadeEnabledPreferenceEntry.GetValue(PreferenceAccessService);
            set => _isAudioCrossFadeEnabledPreferenceEntry.SetValue(PreferenceAccessService, value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="IsAudioCrossfadeEnabled"/>.
        /// </summary>
        private readonly UserPreferenceEntry<bool> _isAudioCrossFadeEnabledPreferenceEntry = new()
        {
            PreferencePath = $"{nameof(Playback)}/{nameof(IsAudioCrossfadeEnabled)}",
            DefaultValue = true
        };

        #endregion

        #region AudioCrossfadeDuration

        /// <summary>
        /// Audio cross-fade duration in seconds.
        /// </summary>
        public double AudioCrossfadeDuration
        {
            get => _audioCrossFadeDurationPreferenceEntry.GetValue(PreferenceAccessService);
            set => _audioCrossFadeDurationPreferenceEntry.SetValue(PreferenceAccessService, value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="AudioCrossfadeDuration"/>.
        /// </summary>
        private readonly UserPreferenceEntry<double> _audioCrossFadeDurationPreferenceEntry = new()
        {
            PreferencePath = $"{nameof(Playback)}/{nameof(AudioCrossfadeDuration)}",
            DefaultValue = 6d
        };

        #endregion

        #region EuqlizerEffectKey
        
        /// <summary>
        /// Equalizer effect key of the player, indicating a specific preset or custom equalizer effect. 
        /// </summary>
        public string EqualizerEffectKey
        {
            get => _equalizerEffectKeyPreferenceEntry.GetValue(PreferenceAccessService);
            set => _equalizerEffectKeyPreferenceEntry.SetValue(PreferenceAccessService, value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="EqualizerEffectKey"/>.
        /// </summary>
        private readonly UserPreferenceEntry<string> _equalizerEffectKeyPreferenceEntry = new()
        {
            PreferencePath = $"{nameof(Playback)}/{nameof(EqualizerEffectKey)}",
            DefaultValue = "Off"
        };
        
        #endregion
        
        #region CustomEqualizerEffect

        /// <summary>
        /// Custom equalizer effect of the player. Must be a <see cref="double"/> array of length 10.
        /// </summary>
        public double[]? CustomEqualizerEffect
        {
            get
            {
                var json = _customEqualizerEffectPreferenceEntry.GetValue(PreferenceAccessService);
                return json is not null &&
                       JsonSerializer.Deserialize<double[]>(json, SerializationContext.Default.DoubleArray) is
                           {Length: 10} result
                    ? result
                    : null;
            }
            set
            {
                if (value?.Length is not 10)
                {
                    _customEqualizerEffectPreferenceEntry.SetValue(PreferenceAccessService, null);
                    return;
                }
                var json = JsonSerializer.Serialize<double[]>(value, SerializationContext.Default.DoubleArray);
                _customEqualizerEffectPreferenceEntry.SetValue(PreferenceAccessService, json);
            }
        }
        
        /// <summary>
        /// Preference entry for <see cref="CustomEqualizerEffect"/>.
        /// </summary>
        private readonly UserPreferenceEntry<string?> _customEqualizerEffectPreferenceEntry = new()
        {
            PreferencePath = $"{nameof(Playback)}/{nameof(CustomEqualizerEffect)}",
            DefaultValue = null
        };

        #endregion

        #region AudioOutputDeviceId

        /// <summary>
        /// Audio output device ID of the player.
        /// </summary>
        public string? AudioOutputDeviceId
        {
            get => _audioOutputDeviceIdPreferenceEntry.GetValue(PreferenceAccessService);
            set => _audioOutputDeviceIdPreferenceEntry.SetValue(PreferenceAccessService, value);
        }
        
        /// <summary>
        /// Preference entry for <see cref="AudioOutputDeviceId"/>
        /// </summary>
        private readonly UserPreferenceEntry<string?> _audioOutputDeviceIdPreferenceEntry = new()
        {
            PreferencePath = $"{nameof(Playback)}/{nameof(AudioOutputDeviceId)}",
            DefaultValue = null
        };

        #endregion
    }

    /// <summary>
    /// Base class for preference containers.
    /// </summary>
    public abstract class UserPreferenceContainer
    {
        /// <summary>
        /// Preference access service from IoC container.
        /// </summary>
        protected readonly IPreferenceAccessService PreferenceAccessService;

        /// <summary>
        /// Constructs the container with preference access service from IoC container.
        /// </summary>
        /// <param name="preferenceAccessService">Preference access service from IoC container.</param>
        protected UserPreferenceContainer(IPreferenceAccessService preferenceAccessService)
        {
            PreferenceAccessService = preferenceAccessService;
        }
    }
    
    /// <summary>
    /// Entry for user preference.
    /// </summary>
    /// <typeparam name="TValue">Type of the preference value.</typeparam>
    private class UserPreferenceEntry<TValue>
    {
        /// <summary>
        /// Path of the preference.
        /// </summary>
        public required string PreferencePath { get; init; }
        
        /// <summary>
        /// Default value of the preference.
        /// </summary>
        public required TValue DefaultValue { get; init; }

        /// <summary>
        /// Whether the entry contains large-sized content that needs external file storage.
        /// </summary>
        public bool IsLarge { get; init; }
        
        /// <summary>
        /// Gets the value of the preference.
        /// </summary>
        /// <param name="preferenceAccessService">Preference access service from IoC container.</param>
        /// <returns>Value of the preference. Default value if not configured.</returns>
        public TValue GetValue(IPreferenceAccessService preferenceAccessService)
        {
            if (preferenceAccessService.TryGetValue(PreferencePath, out TValue? value, IsLarge))
                return value!;
            preferenceAccessService.SetValue(PreferencePath, DefaultValue, IsLarge);
            return DefaultValue;
        }
        
        /// <summary>
        /// Sets the value of the preference.
        /// </summary>
        /// <param name="preferenceAccessService">Preference access service from IoC container.</param>
        /// <param name="value">Value to set.</param>
        public void SetValue(IPreferenceAccessService preferenceAccessService, TValue? value)
        {
            if (EqualityComparer<TValue>.Default.Equals(value, DefaultValue))
                preferenceAccessService.RemoveValue(PreferencePath, IsLarge);
            else preferenceAccessService.SetValue(PreferencePath, value, IsLarge);
        }
        
        /// <summary>
        /// Removes the value of the preference.
        /// </summary>
        /// <param name="preferenceAccessService">Preference access service from IoC container.</param>
        public void RemoveValue(IPreferenceAccessService preferenceAccessService)
        {
            preferenceAccessService.RemoveValue(PreferencePath, IsLarge);
        }
    }
}