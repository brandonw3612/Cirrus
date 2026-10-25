using Cirrus.Base.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Cirrus.ViewModels;

public partial class SidebarViewModel(Action<bool> onIsFullSizeToggledChangedCallback) : ObservableObject
{
    [ObservableProperty] public partial bool IsFullSizeAvailable { get; set; }

    [ObservableProperty] public partial bool IsFullSizeToggled { get; set; } =
        UserPreferenceService.GetService()?.Appearance.IsSidebarOpen ?? false;

    [ObservableProperty] public partial string CurrentFrameContentIdentifier { get; set; } = string.Empty;
        
    partial void OnIsFullSizeToggledChanged(bool value)
    {
        onIsFullSizeToggledChangedCallback(value);
        if (UserPreferenceService.GetService() is { } service)
        {
            service.Appearance.IsSidebarOpen = value;
        }
    }
}