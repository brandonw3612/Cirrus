using Cirrus.Behaviors;
using Cirrus.Commanding.Primitives;
using Cirrus.Primitives;
using Cirrus.ViewModels;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using WinRT;

namespace Cirrus.Views;

public abstract class UserDetailViewBase : View<UserDetailViewModel, ulong>;

[GeneratedBindableCustomProperty([
    nameof(ViewModel)
], [])]
public sealed partial class UserDetailView
{
    private bool _isFloatingHeaderVisible;

    public UserDetailView()
    {
        InitializeComponent();
    }

    [RelayCommand]
    private void OnLoaded()
    {
        var scrollViewer = PlaylistGridView.FindDescendant<ScrollViewer>();
        if (scrollViewer is null) return;
        
        Interaction.SetBehaviors(scrollViewer, [
            new ViewChangedEventTriggerBehavior
            {
                Actions =
                {
                    new InvokeCommandActionEx
                    {
                        Command = new RelayCommandEx<ScrollViewer, ScrollViewerViewChangedEventArgs>((sender, _) =>
                        {
                            UpdateHeaders(sender.VerticalOffset);
                        }) 
                    }
                }
            }
        ]);
        UpdateHeaders(0);
    }
    
    private void UpdateHeaders(double offset)
    {
        var showFloatingHeader = offset > 140;
        if (showFloatingHeader == _isFloatingHeaderVisible) return;
        if (showFloatingHeader)
        {
            ViewHeader.Opacity = 0;
            ViewHeader.IsHitTestVisible = false;
            FloatingHeader.Visibility = Visibility.Visible;
        }
        else
        {
            ViewHeader.Opacity = 1;
            ViewHeader.IsHitTestVisible = true;
            FloatingHeader.Visibility = Visibility.Collapsed;
        }
        _isFloatingHeaderVisible = showFloatingHeader;
    }
}
