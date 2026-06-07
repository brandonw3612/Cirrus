using Cirrus.Utilities;
using Cirrus.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Cirrus.ViewModels;

public partial class LoginDialogContentViewModel : ObservableObject
{
    private readonly ContentDialog _parentDialog;

    public MobileAuthenticator MobileAuthenticator { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    public partial int SelectedLoginMethodIndex { get; set; }

    [ObservableProperty] public partial string CountryCode { get; set; } = "+86";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    public partial string CellphoneNumber { get; set; } = string.Empty;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    public partial string MailAddress { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    public partial string Password { get; set; } = string.Empty;

    public LoginDialogContentViewModel(ContentDialog parentDialog)
    {
        _parentDialog = parentDialog;
        MobileAuthenticator = new(parentDialog.DispatcherQueue, OnMobileAuthenticated);
    }
    
    public bool IsInputValid() => SelectedLoginMethodIndex switch
    {
        0 => CellphoneNumber.Length > 0 && Password.Length > 0,
        1 => MailAddress.Length > 0 && Password.Length > 0,
        _ => false
    };
    
    private async void OnMobileAuthenticated()
    {
        MobileAuthenticator.Deactivate();
        _parentDialog.Hide();
        if (Application.Current is not App { CurrentAccount: { } account }) return;
        await account.ReloadAsync();
    }
    
    [RelayCommand(CanExecute = nameof(IsInputValid))]
    private async Task LoginAsync()
    {
        if (Application.Current is not App { CurrentAccount: { } account }) return;
        if (SelectedLoginMethodIndex is not 0 and not 1) return;
        var result = SelectedLoginMethodIndex is 0
            ? await account.LoginAsync(CellphoneNumber, Password, CountryCode)
            : await account.LoginAsync(MailAddress, Password);
        _parentDialog.Hide();
        if (result.Succeeded || result.Exception is not { } exception) return;
        if (MainWindow.Current is not { } window) return;
        await window.DialogController.ShowMessageBoxAsync(
            "MessageBoxes/Error/Title".GetLocalized() ?? "{Invalid Resource}",
            exception.Message,
            "Controls/Buttons/OK/Content".GetLocalized() ?? "{Invalid Resource}"
        );
    }
    
    [RelayCommand]
    private void Cancel() => _parentDialog.Hide();
    
    partial void OnSelectedLoginMethodIndexChanged(int value)
    {
        CountryCode = "+86";
        CellphoneNumber = string.Empty;
        MailAddress = string.Empty;
        Password = string.Empty;

        if (value is 2)
        {
            MobileAuthenticator.Activate();
        }
        else
        {
            MobileAuthenticator.Deactivate();
        }
    }
}