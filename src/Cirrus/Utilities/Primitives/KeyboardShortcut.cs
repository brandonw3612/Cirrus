using System.Text.Json.Serialization;
using Windows.System;
using Cirrus.Base.Services;
using Cirrus.LiveModels;
using Cirrus.Playback.Extensions;
using Cirrus.Playback.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Cirrus.Utilities.Primitives;

public delegate Task KeyboardShortcutAction();

public partial class KeyboardShortcut : ObservableObject
{
    private static Dictionary<string, KeyboardShortcutAction> SupportedActions { get; } = new()
    {
        ["PlayPause"] = async () =>
        {
            if (ServicesProvider.Current.GetService(typeof(IPlaybackService<ulong>)) is not IPlaybackService<ulong>
                playbackService) return;
            await playbackService.PlayPauseAsync();
        },
        ["Next"] = async () =>
        {
            if (ServicesProvider.Current.GetService(typeof(IPlaybackService<ulong>)) is not IPlaybackService<ulong>
                playbackService) return;
            await playbackService.TryNextAsync();
        },
        ["Previous"] = async () =>
        {
            if (ServicesProvider.Current.GetService(typeof(IPlaybackService<ulong>)) is not IPlaybackService<ulong>
                playbackService) return;
            await playbackService.TryPreviousAsync();
        },
        ["IncreaseVolume"] = () =>
        {
            if (ServicesProvider.Current.GetService(typeof(IPlaybackService<ulong>)) is not IPlaybackService<ulong>
                playbackService) return Task.CompletedTask;
            playbackService.Volume = Math.Clamp(playbackService.Volume + 0.1d, 0d, 1d);
            return Task.CompletedTask;
        },
        ["DecreaseVolume"] = () =>
        {
            if (ServicesProvider.Current.GetService(typeof(IPlaybackService<ulong>)) is not IPlaybackService<ulong>
                playbackService) return Task.CompletedTask;
            playbackService.Volume = Math.Clamp(playbackService.Volume - 0.1d, 0d, 1d);
            return Task.CompletedTask;
        }
    };

    private VirtualKey? _dominantKey;

    public VirtualKey? DominantKey
    {
        get => _dominantKey;
        init => _dominantKey = value;
    }

    private VirtualKey[] _modifierKeys = [];

    public VirtualKey[] ModifierKeys
    {
        get => _modifierKeys;
        init => _modifierKeys = value;
    }

    [ObservableProperty] public partial string ActionKey { get; set; } = string.Empty;

    [JsonIgnore] public bool AreKeysValid => _dominantKey is not null;

    [JsonIgnore]
    public List<VirtualKey> Keys
    {
        get
        {
            List<VirtualKey> keys = [..ModifierKeys];
            if (DominantKey is not null) keys.Add(DominantKey.Value);
            return keys;
        }
        set
        {
            var dominantKeys = value.Where(LabeledVirtualKey.DominantKeyLabels.ContainsKey).ToArray();
            var modifierKeys = value.Select(LabeledVirtualKey.MapModifier)
                .Where(LabeledVirtualKey.ModifierKeyLabels.ContainsKey).Distinct().ToList();
            _dominantKey = dominantKeys.Length > 0 ? dominantKeys[0] : null;
            _modifierKeys = modifierKeys.ToArray();
        }
    }

    [JsonIgnore] public KeyboardShortcutAction? ShortcutAction { get; private set; }

    partial void OnActionKeyChanged(string value) =>
        ShortcutAction = SupportedActions!.GetValueOrDefault(value, null);
}