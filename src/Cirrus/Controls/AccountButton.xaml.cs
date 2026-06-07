using Windows.ApplicationModel;
using Cirrus.Dialogs;
using Cirrus.Views;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Cirrus.Controls;

public sealed partial class AccountButton
{
    public App App { get; }

    public AccountButton()
    {
        App = (Application.Current as App)!;
        InitializeComponent();
    }

    [RelayCommand]
    private void NavigateToUserDetailView()
    {
        if (MainWindow.Current is not { } window || App is not { CurrentAccount.UserAccount: { } account }) return;
        window.Navigate(typeof(UserDetailView), account.UserId);
    }
    
    [RelayCommand]
    private static async Task ShowLoginDialogAsync(RoutedEventArgs args)
    {
        if (Application.Current is not App { CurrentAccount: not null }) return;
        if (MainWindow.Current is not { } window) return;
        ContentDialog dialog = new()
        {
            Title = "Dialogs/ProxyLoginDialog/Title".GetLocalized()
        };
        dialog.Content = new ProxyLoginDialogContent(dialog);
        await window.DialogController.ShowCustomContentDialogAsync(dialog);
    }

    [RelayCommand]
    private async Task LogOutAsync()
    {
        if (MainWindow.Current is not { } window) return;
        var result = await window.DialogController.ShowMessageBoxAsync(
            Package.Current.DisplayName,
            "MessageBoxes/LogOut/Content".GetLocalized() ?? "{Invalid Resource}",
            "MessageBoxes/LogOut/PrimaryButtonText".GetLocalized() ?? "{Invalid Resource}",
            "Controls/Buttons/Cancel/Content".GetLocalized() ?? "{Invalid Resource}"
        );
        if (result is not ContentDialogResult.Primary) return;
        if (App.CurrentAccount is null) return;
        await App.CurrentAccount.LogOutAsync();
    }

    [RelayCommand]
    private static void NavigateToSettingsView()
    {
        if (MainWindow.Current is not { } window) return;
        window.Navigate(typeof(SettingsView));
    }
}