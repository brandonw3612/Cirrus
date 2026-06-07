using Cirrus.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;

namespace Cirrus.ViewModels;

public partial class HistoryViewModel : ViewModel
{
    public override string ViewIdentifier => "History";

    [ObservableProperty] public partial double ViewWidth { get; set; }

    public override Task LoadDataAsync()
    {
        return Task.CompletedTask;
    }

    [RelayCommand]
    private void OnSizeChanged(SizeChangedEventArgs args) => ViewWidth = args.NewSize.Width;
}
