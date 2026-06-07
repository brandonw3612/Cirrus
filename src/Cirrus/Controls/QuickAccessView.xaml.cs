using Cirrus.Dialogs;
using Cirrus.Utilities;
using Cirrus.Views;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Cirrus.Controls;

public sealed partial class QuickAccessView
{
    public NeteaseAccount? Account => (Application.Current as App)?.CurrentAccount;

    public QuickAccessView()
    {
        InitializeComponent();
    }

    [RelayCommand]
    private static async Task ShowLoginDialogAsync()
    {
        if (MainWindow.Current is not { } window) return;
        ContentDialog dialog = new()
        {
            Title = "Dialogs/LoginDialog/Title".GetLocalized(),
        };
        dialog.Content = new LoginDialogContent(dialog);
        await window.DialogController.ShowCustomContentDialogAsync(dialog);
    }
}
