using Windows.System;
using CommunityToolkit.Mvvm.ComponentModel;
using WinRT;

namespace Cirrus.LiveModels;

[GeneratedBindableCustomProperty([
    nameof(Label)
], [])]
public partial class LabeledVirtualKey : ObservableObject
{
    #region Key declarations

    public static readonly Dictionary<VirtualKey, string> ModifierKeyLabels = new()
    {
        [VirtualKey.Control] = "Ctrl",
        [VirtualKey.Shift] = "Shift",
        [VirtualKey.Menu] = "Alt"
    };

    public static readonly Dictionary<VirtualKey, string> DominantKeyLabels = new()
    {
        [VirtualKey.A] = "A",
        [VirtualKey.B] = "B",
        [VirtualKey.C] = "C",
        [VirtualKey.D] = "D",
        [VirtualKey.E] = "E",
        [VirtualKey.F] = "F",
        [VirtualKey.G] = "G",
        [VirtualKey.H] = "H",
        [VirtualKey.I] = "I",
        [VirtualKey.J] = "J",
        [VirtualKey.K] = "K",
        [VirtualKey.L] = "L",
        [VirtualKey.M] = "M",
        [VirtualKey.N] = "N",
        [VirtualKey.O] = "O",
        [VirtualKey.P] = "P",
        [VirtualKey.Q] = "Q",
        [VirtualKey.R] = "R",
        [VirtualKey.S] = "S",
        [VirtualKey.T] = "T",
        [VirtualKey.U] = "U",
        [VirtualKey.V] = "V",
        [VirtualKey.W] = "W",
        [VirtualKey.X] = "X",
        [VirtualKey.Y] = "Y",
        [VirtualKey.Z] = "Z",
        [VirtualKey.Number0] = "0",
        [VirtualKey.Number1] = "1",
        [VirtualKey.Number2] = "2",
        [VirtualKey.Number3] = "3",
        [VirtualKey.Number4] = "4",
        [VirtualKey.Number5] = "5",
        [VirtualKey.Number6] = "6",
        [VirtualKey.Number7] = "7",
        [VirtualKey.Number8] = "8",
        [VirtualKey.Number9] = "9",
        [VirtualKey.Left] = "Left",
        [VirtualKey.Right] = "Right",
        [VirtualKey.Up] = "Up",
        [VirtualKey.Down] = "Down",
        [VirtualKey.Space] = "Space",
    };

    public static VirtualKey MapModifier(VirtualKey key) =>
        key switch
        {
            VirtualKey.LeftControl => VirtualKey.Control,
            VirtualKey.RightControl => VirtualKey.Control,
            VirtualKey.LeftShift => VirtualKey.Shift,
            VirtualKey.RightShift => VirtualKey.Shift,
            VirtualKey.LeftMenu => VirtualKey.Menu,
            VirtualKey.RightMenu => VirtualKey.Menu,
            _ => key
        };

    #endregion

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Label))]
    public partial VirtualKey Key { get; set; }

    public string Label =>
        ModifierKeyLabels.TryGetValue(Key, out var label)
            ? label
            : DominantKeyLabels.GetValueOrDefault(Key, "N/A");
}