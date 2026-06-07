using System.Collections.ObjectModel;
using Cirrus.LiveModels;
using Cirrus.Utilities;
using Cirrus.Utilities.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Cirrus.ViewModels;

public partial class KeyboardShortcutsCustomizationDialogViewModel : ObservableObject
{
    public ObservableCollection<LocalizedText<KeyboardShortcut>> Shortcuts { get; } = new();

    [ObservableProperty] public partial bool IsRecheckRequired { get; set; }

    public void LoadConfig(KeyboardActionHandler config)
    {
        Shortcuts.Clear();
        foreach (var shortcut in config.Shortcuts)
        {
            Shortcuts.Add(new($"KeyboardShortcuts/{shortcut.ActionKey}/Name") { Attachment = shortcut });
        }
    }
}