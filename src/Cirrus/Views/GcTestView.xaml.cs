using System.Text;
using Cirrus.ViewModels;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Cirrus.Views;

public sealed partial class GcTestView : Page
{
    private static readonly (string Name, Type ViewType, object? Parameters)[] TestTargets =
    [
        (nameof(AlbumDetailView), typeof(AlbumDetailView), 97767168UL),
        //(nameof(ArtistDetailView), typeof(ArtistDetailView), 159692UL),
        (nameof(BrowseView), typeof(BrowseView), null),
        (nameof(PlaybackControlView), typeof(PlaybackControlView), null),
        (nameof(CloudDriveView), typeof(CloudDriveView), null),
        (nameof(HistoryView), typeof(HistoryView), null),
        (nameof(LibraryView), typeof(LibraryView), null),
        (nameof(PlaylistDetailView), typeof(PlaylistDetailView), 897784673UL),
        (nameof(SearchResultView), typeof(SearchResultView), "初音未来"),
        (nameof(SearchView), typeof(SearchView), null),
        (nameof(SettingsView), typeof(SettingsView), null),
        (nameof(UserDetailView), typeof(UserDetailView),
            (Application.Current as App)?.CurrentAccount?.UserAccount?.UserId)
    ];

    public GcTestView()
    {
        InitializeComponent();
    }

    private void ForceGcButton_OnClick(object sender, RoutedEventArgs e)
    {
        GC.Collect();
    }

    private async void TestLeaksButton_OnClick(object sender, RoutedEventArgs e)
    {
        TestLeaksButton.IsEnabled = false;
        ForceGcButton.IsEnabled = false;

        try
        {
            var references = await NavigateThroughTestTargetsAsync();
            await Task.Delay(5000);
            GC.Collect();

            var result = new StringBuilder();
            foreach (var (name, reference) in references)
            {
                result.Append(name)
                    .Append(": ")
                    .AppendLine(reference.TryGetTarget(out _) ? "Alive" : "Collected");
            }

            if (MainWindow.Current is not { } window) return;
            await window.DialogController.ShowCustomContentDialogAsync(new ContentDialog
            {
                Title = "Views/GcTestView/ResultDialog/Title".GetLocalized(),
                Content = new ScrollViewer
                {
                    MaxHeight = 520,
                    Content = new TextBlock
                    {
                        FontFamily = new("Consolas"),
                        Text = result.ToString(),
                        TextWrapping = TextWrapping.NoWrap
                    }
                },
                CloseButtonText = "Controls/Buttons/OK/Content".GetLocalized()
            });
        }
        finally
        {
            MainWindow.Current?.SetCurrentViewIdentifier("Settings");
            TestStatusTextBlock.Text = string.Empty;
            TestLeaksButton.IsEnabled = true;
            ForceGcButton.IsEnabled = true;
        }
    }

    private async Task<List<(string Name, WeakReference<FrameworkElement> Reference)>> NavigateThroughTestTargetsAsync()
    {
        var references = new List<(string, WeakReference<FrameworkElement>)>(TestTargets.Length);
        var frame = new Frame
        {
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch
        };
        LeakCheckHost.Child = frame;
        LeakCheckHost.Visibility = Visibility.Visible;

        try
        {
            for (var index = 0; index < TestTargets.Length; index++)
            {
                var target = TestTargets[index];
                TestStatusTextBlock.Text = $"{index + 1}/{TestTargets.Length}: {target.Name}";
                frame.Navigate(target.ViewType, target.Parameters, new SuppressNavigationTransitionInfo());
                await Task.Delay(2000);

                if (frame.Content is FrameworkElement view)
                {
                    references.Add((target.Name, new(view)));
                }
            }
        }
        finally
        {
            LeakCheckHost.Child = null;
            LeakCheckHost.Visibility = Visibility.Collapsed;
        }

        return references;
    }
}
