using Cirrus.Views;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

namespace Cirrus.Utilities;

public class DialogController
{
    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly MainWindow _window;

    public DialogController(MainWindow window)
    {
        _window = window;
    }

    public async Task<ContentDialogResult> ShowMessageBoxAsync(string title, string content, string closeButtonText)
    {
        await _semaphore.WaitAsync();
        try
        {
            return await new ContentDialog
            {
                XamlRoot = _window.XamlRoot,
                Title = title,
                Content = content,
                CloseButtonText = closeButtonText,
                RequestedTheme = _window.CurrentTheme
            }.ShowAsync();
        }
        catch
        {
            return ContentDialogResult.None;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task<ContentDialogResult> ShowMessageBoxAsync(string title, string content,
        string primaryButtonText, string closeButtonText, Action? primaryAction = null)
    {
        await _semaphore.WaitAsync();
        try
        {
            ContentDialog dialog = new()
            {
                XamlRoot = _window.XamlRoot,
                Title = title,
                Content = content,
                DefaultButton = ContentDialogButton.Primary,
                PrimaryButtonText = primaryButtonText,
                CloseButtonText = closeButtonText,
                RequestedTheme = _window.CurrentTheme
            };
            if (primaryAction is not null) dialog.PrimaryButtonCommand = new RelayCommand(primaryAction);
            return await dialog.ShowAsync();
        }
        catch
        {
            return ContentDialogResult.None;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<ContentDialogResult> ShowCustomContentDialogAsync(ContentDialog dialog)
    {
        dialog.XamlRoot = _window.XamlRoot;
        dialog.RequestedTheme = _window.CurrentTheme;
        await _semaphore.WaitAsync();
        try
        {
            return await dialog.ShowAsync();
        }
        catch
        {
            return ContentDialogResult.None;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}