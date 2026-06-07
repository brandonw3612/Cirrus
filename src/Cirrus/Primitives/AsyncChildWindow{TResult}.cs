using Windows.UI;
using Cirrus.Base.Services;
using Cirrus.Interops;
using Cirrus.Models.Business.Appearance;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinUIEx;

namespace Cirrus.Primitives;

public abstract class AsyncChildWindow<TResult> : WindowEx
{
    private TaskCompletionSource<TResult?>? _taskCompletionSource;
    private Window? _parentWindow;

    protected abstract Task OnInitializeAsync();
    
    public Task<TResult?> ShowAtAsync(Window parentWindow)
    {
        _parentWindow = parentWindow;
        Closed += OnClosed;
        if (AppWindow.Presenter is OverlappedPresenter p)
        {
            WindowingInterop.EnableWindow(parentWindow.GetWindowHandle(), false);
            WindowingInterop.SetWindowLong(this.GetWindowHandle(), WindowingInterop.GetWindowLongHwndParent,
                parentWindow.GetWindowHandle());
            p.IsMinimizable = false;
            p.IsMaximizable = false;
            p.IsModal = true;
        }
        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
        AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        if (ServicesProvider.GetService<UserPreferenceService>() is { } userPreferenceService)
        {
            var theme = userPreferenceService.Appearance.DisplayTheme;
            if (Content is FrameworkElement content)
            {
                content.RequestedTheme = theme switch
                {
                    DisplayTheme.Light => ElementTheme.Light,
                    DisplayTheme.Dark => ElementTheme.Dark,
                    _ => ElementTheme.Default
                };
            }
            AppWindow.TitleBar.ButtonForegroundColor = theme switch
            {
                DisplayTheme.Light => Color.FromArgb(255, 0, 0, 0),
                DisplayTheme.Dark => Color.FromArgb(255, 255, 255, 255),
                _ => null
            };
        }
        _taskCompletionSource = new();
        _ = OnInitializeAsync();
        this.Show();
        return _taskCompletionSource.Task;
    }

    protected void ReturnWith(TResult? result)
    {
        if (_taskCompletionSource is null || _taskCompletionSource.Task.IsCompleted) return;
        _taskCompletionSource.SetResult(result);
        Close();
    }
    
    private void OnClosed(object sender, WindowEventArgs args)
    {
        Closed -= OnClosed;
        if (_parentWindow is not null) WindowingInterop.EnableWindow(_parentWindow.GetWindowHandle(), true);
        if (_taskCompletionSource is null || _taskCompletionSource.Task.IsCompleted) return;
        _taskCompletionSource.SetResult(default);
    }
}