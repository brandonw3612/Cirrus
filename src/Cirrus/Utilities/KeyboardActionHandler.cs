using System.Text.Json;
using Windows.Storage;
using Windows.System;
using Cirrus.Behaviors;
using Cirrus.LiveModels;
using Cirrus.Serialization;
using Cirrus.Utilities.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Cirrus.Utilities;

public partial class KeyboardActionHandler : ObservableObject
{
    private readonly HashSet<VirtualKey> _currentKeys = new();
    
    public KeyboardShortcut[] Shortcuts { get; }
    
    private KeyboardActionHandler(KeyboardShortcut[] shortcuts)
    {
        Shortcuts = shortcuts;
    }

    #region Element attachment and detachment

    public void Attach(UIElement? target)
    {
        if (target is null) return;
        target.DetachBehavior<PreviewKeyDownEventTriggerBehavior>();
        target.AttachEventTriggerBehaviorToCommand<UIElement, PreviewKeyDownEventTriggerBehavior>
            (PreviewKeyDownCommand);
        target.DetachBehavior<PreviewKeyUpEventTriggerBehavior>();
        target.AttachEventTriggerBehaviorToCommand<UIElement, PreviewKeyUpEventTriggerBehavior>
            (PreviewKeyUpCommand);
        target.KeyboardAccelerators.Clear();
        foreach (var shortcut in Shortcuts)
        {
            if (!shortcut.AreKeysValid) continue;
            KeyboardAccelerator accelerator = new()
            {
                Key = shortcut.DominantKey!.Value,
                Modifiers = shortcut.ModifierKeys.Aggregate(VirtualKeyModifiers.None, (current, key) => current |
                    key switch
                    {
                        VirtualKey.Control => VirtualKeyModifiers.Control,
                        VirtualKey.Menu => VirtualKeyModifiers.Menu,
                        VirtualKey.Shift => VirtualKeyModifiers.Shift,
                        _ => VirtualKeyModifiers.None
                    })
            };
            accelerator.AttachEventTriggerBehaviorToCommand<KeyboardAccelerator, InvokedEventTriggerBehavior>(
                new AsyncRelayCommand<KeyboardAcceleratorInvokedEventArgs>(async args =>
                {
                    if (IsInputControlFocused(FocusManager.GetFocusedElement(target.XamlRoot))) return;
                    if (shortcut.ShortcutAction is null) return;
                    if (args is not null) args.Handled = true;
                    await shortcut.ShortcutAction.Invoke();
                }));
            target.KeyboardAccelerators.Add(accelerator);
        }
        target.KeyboardAcceleratorPlacementMode = KeyboardAcceleratorPlacementMode.Hidden;
    }

    private static bool IsInputControlFocused(object? element)
    {
        return element is TextBox
            or PasswordBox
            or RichEditBox
            or AutoSuggestBox
            or NumberBox;
    }

    public void Detach(UIElement target)
    {
        target.DetachBehavior<PreviewKeyDownEventTriggerBehavior>();
        target.DetachBehavior<PreviewKeyUpEventTriggerBehavior>();
        target.KeyboardAccelerators.Clear();
    }

    [RelayCommand]
    private async Task OnPreviewKeyDown(KeyRoutedEventArgs args)
    {
        if (IsInputControlFocused(args.OriginalSource)) return;
        _currentKeys.Add(LabeledVirtualKey.MapModifier(args.Key));
        if (Shortcuts.FirstOrDefault(s => _currentKeys.SetEquals(s.Keys)) is { } shortcut)
        {
            if (shortcut.ShortcutAction != null) await shortcut.ShortcutAction.Invoke();
            args.Handled = true;
        }
    }
    
    [RelayCommand]
    private void OnPreviewKeyUp(KeyRoutedEventArgs args) => _currentKeys.Remove(args.Key);

    #endregion
    
    public static async Task<KeyboardActionHandler> CreateAsync() => new(await LoadLocalConfigAsync());

    public Task SaveAsync() => SaveLocalConfigAsync(Shortcuts);
    
    public static KeyboardActionHandler CreateDefault() => new(DefaultShortcuts);
    
    #region Default shortcut set

    private static KeyboardShortcut[] DefaultShortcuts =>
    [
        new()
        {
            ActionKey = "PlayPause",
            DominantKey = VirtualKey.Space
        },
        new()
        {
            ActionKey = "Previous",
            ModifierKeys = [VirtualKey.Control],
            DominantKey = VirtualKey.Left
        },
        new()
        {
            ActionKey = "Next",
            ModifierKeys = [VirtualKey.Control],
            DominantKey = VirtualKey.Right
        },
        new()
        {
            ActionKey = "IncreaseVolume",
            ModifierKeys = [VirtualKey.Control],
            DominantKey = VirtualKey.Up
        },
        new()
        {
            ActionKey = "DecreaseVolume",
            ModifierKeys = [VirtualKey.Control],
            DominantKey = VirtualKey.Down
        }
    ];

    #endregion
    
    #region Local cache operations
    
    private static async Task<KeyboardShortcut[]> LoadLocalConfigAsync()
    {
        try
        {
            var localFolder = ApplicationData.Current.LocalFolder.Path;
            var cacheFilePath = Path.Join(localFolder, "Config", "KeyboardShortcuts.json");
            if (!File.Exists(cacheFilePath)) throw new Exception();
            var fileContent = await File.ReadAllTextAsync(cacheFilePath);
            return JsonSerializer.Deserialize<KeyboardShortcut[]>(fileContent,
                AppSerializationContext.Default.KeyboardShortcutArray) ?? DefaultShortcuts;
        }
        catch
        {
            return DefaultShortcuts;
        }
    }

    private static async Task SaveLocalConfigAsync(KeyboardShortcut[] shortcuts)
    {
        try
        {
            var localFolder = ApplicationData.Current.LocalFolder.Path;
            var cacheFolder = Path.Join(localFolder, "Config");
            Directory.CreateDirectory(cacheFolder);
            var cacheFilePath = Path.Join(cacheFolder, "KeyboardShortcuts.json");
            var fileContent = JsonSerializer.Serialize(shortcuts, AppSerializationContext.Default.KeyboardShortcutArray);
            await File.WriteAllTextAsync(cacheFilePath, fileContent);
        }
        catch
        {
            // Ignored.
        }
    }
    
    #endregion
}