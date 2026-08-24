using Cirrus.Base.Services;
using Cirrus.Base.Services.Abstract;
using Cirrus.Network;
using Cirrus.Playback.PlaybackServices;
using Cirrus.Playback.Primitives;
using Cirrus.Services;
using Cirrus.Utilities;
using Cirrus.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.AppLifecycle;

namespace Cirrus;

public sealed partial class App
{
    public MainWindow? MainWindow { get; private set; }

    public NeteaseAccount? CurrentAccount { get; private set; }
    
    public App()
    {
        InitializeComponent();
        ServicesProvider.TryRegisterServices(static col =>
            col.AddSingleton<IPreferenceAccessService, PreferenceAccessService>()
                .AddSingleton<ILocalizationService, LocalizationService>()
                .AddSingleton<ISynchronizationContextService>(new SynchronizationContextService(SynchronizationContext.Current!))
                .AddSingleton<IPlaybackService<ulong>, SerenadePlaybackService>()
                .AddSingleton<IConfigurationService, ConfigurationService>()
                // .AddSingleton<ISoftwareUpdateService, SoftwareUpdateService>()
                // .RegisterHttpClientForConsumerService<SoftwareUpdateService>()
        );
        if (ServicesProvider.GetService<UserPreferenceService>() is not { } userPreferenceService) return;
        Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride =
            LanguageUtility.GetSupportedSystemLanguage(userPreferenceService.General.DisplayLanguage);
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        if (ServicesProvider.GetService<UserPreferenceService>() is not { } userPreferenceService) return;
        if (userPreferenceService.Appearance.DisplayFont is { } fontName)
        {
            FontFamily font = new(fontName);
            Resources["ContentControlThemeFontFamily"] = font;
            Resources.MergedDictionaries.Add(XamlReader.Load("""
                <ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                    <Style TargetType="MenuFlyoutPresenter" BasedOn="{StaticResource DefaultMenuFlyoutPresenterStyle}">
                        <Setter Property="FontFamily" Value="{StaticResource ContentControlThemeFontFamily}" />
                    </Style>
                    <Style TargetType="MenuFlyoutItem" BasedOn="{StaticResource DefaultMenuFlyoutItemStyle}">
                        <Setter Property="FontFamily" Value="{StaticResource ContentControlThemeFontFamily}" />
                    </Style>
                    <Style TargetType="MenuFlyoutSubItem" BasedOn="{StaticResource DefaultMenuFlyoutSubItemStyle}">
                        <Setter Property="FontFamily" Value="{StaticResource ContentControlThemeFontFamily}" />
                    </Style>
                    <Style TargetType="ToggleMenuFlyoutItem" BasedOn="{StaticResource DefaultToggleMenuFlyoutItemStyle}">
                        <Setter Property="FontFamily" Value="{StaticResource ContentControlThemeFontFamily}" />
                    </Style>
                </ResourceDictionary>
                """) as ResourceDictionary);
        }
        var currentInstance = AppInstance.GetCurrent();
        var instances = AppInstance.GetInstances();
        if (instances.Count > 1)
        {
            var activatedEventArgs = currentInstance.GetActivatedEventArgs();
            if (instances.SingleOrDefault(static i => !i.IsCurrent) is { } instance)
            {
                await instance.RedirectActivationToAsync(activatedEventArgs);
            }
            Environment.Exit(0);
        }
        currentInstance.Activated += OnActivated;
        await Client.Initialize();
        CurrentAccount = await NeteaseAccount.LoadAsync();
        MainWindow = new MainWindow();
        MainWindow.Activate();
        if (ServicesProvider.Current.GetService<IPlaybackService<ulong>>() is SerenadePlaybackService playbackService)
        {
            playbackService.InitializeSystemMediaTransportControls(MainWindow);
            await playbackService.InitializeAsync();
        }
    }
    
    private void OnActivated(object? sender, AppActivationArguments e)
    {
        // Temporarily ignored.
    }
}