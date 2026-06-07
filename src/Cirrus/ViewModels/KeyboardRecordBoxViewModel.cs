using Windows.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using WinRT;

namespace Cirrus.ViewModels;

[GeneratedBindableCustomProperty]
public partial class KeyboardRecordBoxViewModel : ObservableObject
{
    [ObservableProperty] public partial bool IsKeysStatusVisible { get; set; }

    [ObservableProperty] public partial bool IsWarningVisible { get; set; }

    [ObservableProperty] public partial Color KeysStatusBackgroundColor { get; set; } = Colors.Transparent;

    [ObservableProperty] public partial string KeysStatusFontIconGlyph { get; set; } = string.Empty;

    [ObservableProperty] public partial string WarningText { get; set; } = string.Empty;
}