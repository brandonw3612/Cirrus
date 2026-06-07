using Cirrus.Behaviors;
using Cirrus.Primitives;
using Cirrus.ViewModels;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Cirrus.Dialogs;

public abstract class ProxyLoginDialogContentBase : View<ProxyLoginDialogContentViewModel>;

public sealed partial class ProxyLoginDialogContent
{
    private readonly ContentDialog _parentDialog;

    public ProxyLoginDialogContent(ContentDialog parentDialog)
    {
        _parentDialog = parentDialog;
        InitializeComponent();
        this.AttachEventTriggerBehaviorToCommand<FrameworkElement, LoadedEventTriggerBehavior>(ViewModel.StartProxyCommand);
    }

    [RelayCommand]
    private async Task Finish()
    {
        await ViewModel.Complete();
        _parentDialog.Hide();
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await ViewModel.Cancel();
        _parentDialog.Hide();
    }
}