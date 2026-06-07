using CommunityToolkit.Mvvm.ComponentModel;

namespace Cirrus.ViewModels;

public partial class CirculatedFlipViewPanelViewModel : ObservableObject
{
    [ObservableProperty] public partial int CurrentLeadingIndex { get; set; }
}