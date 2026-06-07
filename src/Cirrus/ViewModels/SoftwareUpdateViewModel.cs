using System.Collections.ObjectModel;
using Windows.UI;
using Cirrus.Base.Services;
using Cirrus.Base.Services.Abstract;
using Cirrus.Commanding;
using Cirrus.Models.Business.Developer;
using Cirrus.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.UI;

namespace Cirrus.ViewModels;

public partial class SoftwareUpdateViewModel : ViewModel
{
    public override string ViewIdentifier => "SoftwareUpdate";

    public ObservableCollection<ISoftwareUpdate> AvailableUpdates { get; } = new();

    [ObservableProperty] public partial Color IndicatorBackgroundColor { get; set; } = Colors.Transparent;
    [ObservableProperty] public partial string IndicatorText { get; set; } = string.Empty;
    [ObservableProperty] public partial string IndicatorIconGlyph { get; set; } = string.Empty;
    [ObservableProperty] public partial string LastCheckedTime { get; set; } = string.Empty;
    [ObservableProperty] public partial bool IsChecking { get; set; }
    
    public bool ArePreviewBuildsEnabled
    {
        get => _userPreferenceService!.General.ArePreviewBuildsEnabled;
        set => _userPreferenceService!.General.ArePreviewBuildsEnabled = value; 
    }
    
    private UserPreferenceService? _userPreferenceService;
    private ISoftwareUpdateService? _softwareUpdateService;

    public SoftwareUpdateViewModel()
    {
        SetIndicatorStatus(IndicatorStatus.Checking);
        LastCheckedTime = string.Empty;
        IsChecking = false;
    }
    
    public override async Task LoadDataAsync()
    {
        _userPreferenceService = ServicesProvider.GetService<UserPreferenceService>();
        if (_userPreferenceService is null) throw new Exception("Internal fatal error: Could not load preference service.");
        _softwareUpdateService = ISoftwareUpdateService.GetService();
        if (_softwareUpdateService is null) throw new Exception("Internal fatal error: Could not load software update service.");
        await GetUpdates();
    }

    [RelayCommand]
    private void DownloadUpdate(ISoftwareUpdate update)
    {
        NavigateCommand.Instance.Execute(update);
        // AppCenterService.AppCenterInlineUpdateTrack(update, "Bare");
    }
    
    [RelayCommand]
    private async Task GetUpdates()
    {
        IsChecking = true;
        if (_softwareUpdateService is null)
        {
            SetIndicatorStatus(IndicatorStatus.ServiceError);
            IsChecking = false;
            return;
        }
        SetIndicatorStatus(IndicatorStatus.Checking);
        var updates = _softwareUpdateService.FetchUpdatesAsync(ArePreviewBuildsEnabled);
        AvailableUpdates.Clear();
        List<ISoftwareUpdate> updateList = new();
        await foreach (var update in updates)
        {
            updateList.Add(update);
        }
        if (updateList.Count is 0)
        {
            SetIndicatorStatus(IndicatorStatus.ServiceError);
            IsChecking = false;
            return;
        }
        foreach (var update in updateList.OrderDescending())
        {
            AvailableUpdates.Add(update);
        }
        SetIndicatorStatus(AvailableUpdates[0].IsUpdatable
            ? IndicatorStatus.UpdatesAvailable
            : IndicatorStatus.UpToDate);
        var updatedTime = DateTimeOffset.Now;
        _userPreferenceService!.General.UpdateCheckedTime = updatedTime;
        LastCheckedTime =
            string.Format("Views/SoftwareUpdateView/UpdateCheckedTimeFormatString".GetLocalized() ?? "{0}",
                updatedTime.ToLocalTime().ToString("g"));
        IsChecking = false;
    }

    private void SetIndicatorStatus(IndicatorStatus status)
    {
        IndicatorBackgroundColor = status switch
        {
            IndicatorStatus.UpToDate => Color.FromArgb(100, 82, 204, 0),
            IndicatorStatus.Checking => Color.FromArgb(100, 0, 163, 204),
            IndicatorStatus.UpdatesAvailable => Color.FromArgb(100, 204, 163, 0),
            _ => Color.FromArgb(100, 204, 0, 0)
        };

        var textKey = status switch
        {
            IndicatorStatus.UpToDate => "UpToDate",
            IndicatorStatus.Checking => "Checking",
            IndicatorStatus.UpdatesAvailable => "UpdatesAvailable",
            _ => "ServiceError"
        };
        IndicatorText = ("Views/SoftwareUpdateView/IndicatorText/" + textKey).GetLocalized() ?? "{Invalid Resource}";

        IndicatorIconGlyph = status switch
        {
            IndicatorStatus.UpToDate => "\uE920",
            IndicatorStatus.Checking => "\uE921",
            IndicatorStatus.UpdatesAvailable => "\uE922",
            _ => "\uE923"
        };
    }
    
    private enum IndicatorStatus
    {
        UpToDate, UpdatesAvailable, ServiceError, Checking
    }
}