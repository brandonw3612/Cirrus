using Cirrus.Base.Primitives;
using Cirrus.Base.Services;
using Cirrus.Base.Services.Abstract;
using Cirrus.ViewModels;
using Cirrus.Views;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Cirrus.Primitives;

public abstract class View<TViewModel, TParameter> : Page
    where TViewModel : ViewModel<TParameter>, new()
{
    public TViewModel ViewModel
    {
        get => (TViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(TViewModel), typeof(View<TViewModel, TParameter>),
            new PropertyMetadata(new TViewModel()));

    protected View()
    {
        ViewModel = new();
        NavigationCacheMode = NavigationCacheMode.Disabled;
    }
    
    protected virtual Task LoadVisualContentAsync()
    {
        // Do nothing.
        return Task.CompletedTask;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var window = MainWindow.Current!;
        try
        {
            switch (e.Parameter)
            {
                case TViewModel viewModel:
                    ViewModel = viewModel;
                    window.SetCurrentViewIdentifier(ViewModel.ViewIdentifier);
                    break;
                case TParameter parameter:
                    ViewModel = new()
                    {
                        Parameter = parameter
                    };
                    window.SetCurrentViewIdentifier(ViewModel.ViewIdentifier);
                    // When the passed-in parameter is not a ViewModel instance can we say we're on a new page,
                    // and thus we should push the current page onto the navigation stack.
                    if (e.Parameter is not TViewModel) window.NavigationStack.Push(GetType(), ViewModel);
                    // Otherwise we are doing a backward navigation and we simply just do nothing.
                    ViewModel.IsLoading = true;
                    await ViewModel.LoadDataAsync();
                    break;
                default:
#if DEBUG
                    throw new Exception(
                        $"Navigation parameter type: <{e.Parameter.GetType()}>. " +
                        $"Expected: <{typeof(TViewModel)}> or <null>.");
#else
                    return;
#endif
            }
            ViewModel.AllocateDisposable();
            await LoadVisualContentAsync();
        }
        catch (Exception exception)
        {
            switch (exception)
            {
                case ILoggableException loggableException:
                {
                    if (ServicesProvider.GetService<UserPreferenceService>() is { General.SendDiagnosticsData: true } &&
                        IEventLogService.GetService() is { } eventLogService)
                    {
                        var log = loggableException.GenerateLog();
                        await eventLogService.LogErrorAsync(log.Exception, log.Properties);
                    }
                    await window.DialogController.ShowMessageBoxAsync(
                        "MessageBoxes/Error/Title".GetLocalized() ?? "{InvalidResource}",
                        "MessageBoxes/PageLoadUnsolvableIssue/Content".GetLocalized() ?? "{InvalidResource}",
                        "Controls/Buttons/NavigateBack/Content".GetLocalized() ?? "{InvalidResource}"
                    );
                    MainWindowViewModel.NavigateBack();
                    break;
                }
                default:
                {
                    switch (await window.DialogController.ShowMessageBoxAsync(
                                "MessageBoxes/Error/Title".GetLocalized() ?? "{InvalidResource}",
                                string.Format("MessageBoxes/PageLoadNetworkIssue/Content/Format".GetLocalized() ?? "{InvalidResource}", exception.Message),
                                "Controls/Buttons/Reload/Content".GetLocalized() ?? "{InvalidResource}",
                                "Controls/Buttons/NavigateBack/Content".GetLocalized() ?? "{InvalidResource}"
                            ))
                    {
                        case ContentDialogResult.Primary: await RefreshViewAsync(); break;
                        default: MainWindowViewModel.NavigateBack(); break;
                    }
                    break;
                }
            }
        }
        finally
        {
            ViewModel.IsLoading = false;
        }
        
        base.OnNavigatedTo(e);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        ViewModel.RecycleDisposable();
        base.OnNavigatedFrom(e);
    }

    public async Task RefreshViewAsync()
    {
        var parameter = ViewModel.Parameter;
        ViewModel = new() { Parameter = parameter };
        await ViewModel.LoadDataAsync();
        await LoadVisualContentAsync();
    }
}