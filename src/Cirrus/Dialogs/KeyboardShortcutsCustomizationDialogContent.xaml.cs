using Cirrus.Primitives;
using Cirrus.Utilities;
using Cirrus.ViewModels;
using CommunityToolkit.Mvvm.Input;

namespace Cirrus.Dialogs;

public abstract class KeyboardShortcutsCustomizationDialogContentBase
    (KeyboardShortcutsCustomizationDialogViewModel viewModel) : 
    ModeledView<KeyboardShortcutsCustomizationDialogViewModel>(viewModel);

public sealed partial class KeyboardShortcutsCustomizationDialogContent
{
    public KeyboardShortcutsCustomizationDialogContent() : base(new())
    {
        InitializeComponent();
    }

    public bool TryExportConfig(out KeyboardActionHandler config)
    {
        config = KeyboardActionHandler.CreateDefault();
        foreach (var shortcut in ViewModel.Shortcuts)
        {
            if (shortcut.Attachment is null) continue;
            if (!shortcut.Attachment.AreKeysValid)
            {
                ViewModel.IsRecheckRequired = true;
                return false;
            }
            ViewModel.IsRecheckRequired = false;
            var configShortcut = config.Shortcuts.FirstOrDefault(s => s.ActionKey == shortcut.Attachment.ActionKey);
            configShortcut?.Keys = shortcut.Attachment.Keys;
        }
        return true;
    }

    public async Task LoadConfigAsync() => ViewModel.LoadConfig(await KeyboardActionHandler.CreateAsync());
    
    [RelayCommand]
    private void LoadDefaultConfig() => ViewModel.LoadConfig(KeyboardActionHandler.CreateDefault());
}