using System.Collections.ObjectModel;
using Windows.System;
using Cirrus.LiveModels;
using Cirrus.ViewModels;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using WinRT;

namespace Cirrus.Controls;

[GeneratedBindableCustomProperty([
    nameof(DisplayedKeys),
    nameof(ViewModel),
    nameof(ClearKeysCommand)
], [])]
public sealed partial class KeyboardRecordBox : Control
{
    public KeyboardRecordBoxViewModel ViewModel { get; } = new();
    
    private readonly List<VirtualKey> _keysPressed = [];

    public List<VirtualKey> Keys
    {
        get => (List<VirtualKey>)GetValue(KeysProperty);
        set => SetValue(KeysProperty, value);
    }
    public static readonly DependencyProperty KeysProperty =
        DependencyProperty.Register(nameof(Keys), typeof(List<VirtualKey>), typeof(KeyboardRecordBox),
            new PropertyMetadata(new List<VirtualKey>(), OnKeysChanged));

    public ObservableCollection<LabeledVirtualKey> DisplayedKeys { get; } = [];
    
    public KeyboardRecordBox()
    {
        DefaultStyleKey = typeof(KeyboardRecordBox);
    }
    
    private static void OnKeysChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not KeyboardRecordBox box) return;
        if (e.NewValue is not List<VirtualKey> keys) return;
        box.SetDisplay(keys);
    }
    
    public IRelayCommand ClearKeysCommand => field ??= new RelayCommand(ClearKeys);
    private void ClearKeys()
    {
        _keysPressed.Clear();
        Keys = [];
        ViewModel.IsWarningVisible = ViewModel.IsKeysStatusVisible = false;
    }

    private static int ConvertKey(VirtualKey key) =>
        key switch
        {
            VirtualKey.Control => 0,
            VirtualKey.Menu => 1,
            VirtualKey.Shift => 2,
            _ => 3
        };

    private void SetDisplay(List<VirtualKey> keys)
    {
        DisplayedKeys.Clear();
        keys.Select(LabeledVirtualKey.MapModifier)
            .Distinct()
            .OrderBy(ConvertKey)
            .Select(k => new LabeledVirtualKey { Key = k })
            .ToList()
            .ForEach(DisplayedKeys.Add);
        ViewModel.IsKeysStatusVisible = true;
        if (keys.Count(k => LabeledVirtualKey.DominantKeyLabels.ContainsKey(k)) is 1)
        {
            ViewModel.KeysStatusFontIconGlyph = "\xE8FB";
            ViewModel.KeysStatusBackgroundColor = Colors.ForestGreen;
        }
        else
        {
            ViewModel.KeysStatusFontIconGlyph = "\xE711";
            ViewModel.KeysStatusBackgroundColor = Colors.Crimson;
        }
    }

    protected override void OnPreviewKeyDown(KeyRoutedEventArgs e)
    {
        e.Handled = true;
        ViewModel.IsWarningVisible = false;
        if (!LabeledVirtualKey.DominantKeyLabels.ContainsKey(e.Key) && !LabeledVirtualKey.ModifierKeyLabels.ContainsKey(e.Key))
        {
            ViewModel.IsKeysStatusVisible = false;
            ViewModel.IsWarningVisible = true;
            ViewModel.WarningText = "Controls/KeyboardRecordBox/WarningText".GetLocalized()!;
            _keysPressed.Clear();
            Keys.Clear();
            DisplayedKeys.Clear();
            return;
        }
        if (!_keysPressed.Contains(e.Key)) _keysPressed.Add(e.Key);
        Keys = [.._keysPressed];
        base.OnPreviewKeyDown(e);
    }

    protected override void OnPreviewKeyUp(KeyRoutedEventArgs e)
    {
        e.Handled = true;
        if (_keysPressed.Contains(e.Key)) _keysPressed.Remove(e.Key);
        base.OnPreviewKeyUp(e);
    }
}