using Cirrus.Models.Abstract;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Cirrus.Models.Business.User;

public partial class TopChart : ObservableObject, ITopChart
{
    [ObservableProperty] public partial ulong UserId { get; set; }
    [ObservableProperty] public partial bool IsAccessible { get; set; }
    [ObservableProperty] public partial string Nickname { get; set; } = string.Empty;
    [ObservableProperty] public partial long PlayedTrackCount { get; set; }
}